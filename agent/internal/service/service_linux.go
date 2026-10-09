//go:build linux

package service

import (
	"fmt"
	"log/slog"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
)

// systemd 单元文件存放目录
const systemdUnitDir = "/etc/systemd/system"

type linuxManager struct{}

func newManager() Manager {
	return &linuxManager{}
}

// unitPath 返回服务对应的 systemd 单元文件路径
func (m *linuxManager) unitPath(serviceName string) string {
	return filepath.Join(systemdUnitDir, serviceName+".service")
}

// runSystemctl 执行 systemctl 命令并返回输出
func runSystemctl(args ...string) (string, error) {
	cmd := exec.Command("systemctl", args...)
	out, err := cmd.CombinedOutput()
	output := string(out)
	if err != nil {
		return output, fmt.Errorf("systemctl %s 执行失败: %v, 输出: %s",
			strings.Join(args, " "), err, strings.TrimSpace(output))
	}
	return output, nil
}

// buildExecStart 根据程序路径构建 systemd 的 ExecStart 配置
// 若为 .dll 文件则使用 dotnet 启动，否则直接执行程序
func buildExecStart(exePath, exeParams string) string {
	exeParams = strings.TrimSpace(exeParams)
	if strings.HasSuffix(strings.ToLower(exePath), ".dll") {
		if exeParams == "" {
			return "dotnet " + exePath
		}
		return fmt.Sprintf("dotnet %s %s", exePath, exeParams)
	}
	if exeParams == "" {
		return exePath
	}
	return fmt.Sprintf("%s %s", exePath, exeParams)
}

// Install 生成 systemd 单元文件并启用服务
func (m *linuxManager) Install(serviceName, exePath, exeParams, workDir string) error {
	// 确保程序具有可执行权限
	if err := EnsureExecutable(exePath); err != nil {
		slog.Warn("设置可执行权限失败", "exePath", exePath, "error", err)
	}

	content := fmt.Sprintf(`[Unit]
Description=%s
After=network.target

[Service]
Type=simple
WorkingDirectory=%s
ExecStart=%s
Restart=always
RestartSec=5
KillSignal=SIGINT

[Install]
WantedBy=multi-user.target
`, serviceName, workDir, buildExecStart(exePath, exeParams))

	slog.Info("写入 systemd 单元文件", "path", m.unitPath(serviceName))
	if err := os.WriteFile(m.unitPath(serviceName), []byte(content), 0644); err != nil {
		return fmt.Errorf("写入 systemd 单元文件失败: %v", err)
	}

	if _, err := runSystemctl("daemon-reload"); err != nil {
		return err
	}
	if _, err := runSystemctl("enable", serviceName); err != nil {
		return err
	}
	return nil
}

// Start 启动服务
func (m *linuxManager) Start(serviceName string) error {
	_, err := runSystemctl("start", serviceName)
	return err
}

// Stop 停止服务
func (m *linuxManager) Stop(serviceName string) error {
	_, err := runSystemctl("stop", serviceName)
	return err
}

// Delete 停止、禁用并删除服务
func (m *linuxManager) Delete(serviceName string) error {
	// 忽略停止/禁用时的错误（服务可能已停止或未启用）
	if _, err := runSystemctl("stop", serviceName); err != nil {
		slog.Warn("停止服务失败，继续删除", "service", serviceName, "error", err)
	}
	if _, err := runSystemctl("disable", serviceName); err != nil {
		slog.Warn("禁用服务失败，继续删除", "service", serviceName, "error", err)
	}

	if err := os.Remove(m.unitPath(serviceName)); err != nil && !os.IsNotExist(err) {
		return fmt.Errorf("删除 systemd 单元文件失败: %v", err)
	}
	if _, err := runSystemctl("daemon-reload"); err != nil {
		return err
	}
	slog.Info("服务删除成功", "service", serviceName)
	return nil
}

// IsRunning 判断服务是否正在运行
func (m *linuxManager) IsRunning(serviceName string) (bool, error) {
	if _, err := os.Stat(m.unitPath(serviceName)); err != nil {
		return false, fmt.Errorf("服务不存在: %s", serviceName)
	}
	return systemdState(serviceName) == "active", nil
}

// GetStatusText 获取服务的可读状态
func (m *linuxManager) GetStatusText(serviceName string) (string, error) {
	if _, err := os.Stat(m.unitPath(serviceName)); err != nil {
		return "", fmt.Errorf("服务不存在: %s", serviceName)
	}
	return linuxStatusText(systemdState(serviceName)), nil
}

// systemdState 通过 systemctl is-active 获取服务状态
// is-active 在服务未运行时返回非零退出码，但输出仍有效，因此忽略错误
func systemdState(serviceName string) string {
	cmd := exec.Command("systemctl", "is-active", serviceName)
	out, _ := cmd.Output()
	return strings.TrimSpace(string(out))
}

// linuxStatusText 将 systemd 状态转换为与 Windows 端一致的中文文本
func linuxStatusText(state string) string {
	switch state {
	case "active":
		return "正在运行"
	case "activating":
		return "正在启动"
	case "deactivating":
		return "正在停止"
	case "inactive", "failed", "dead":
		return "已停止"
	default:
		if state == "" {
			return "未知状态"
		}
		return state
	}
}

// EnsureExecutable 确保程序具有可执行权限
func EnsureExecutable(path string) error {
	if path == "" {
		return nil
	}
	// .dll 通过 dotnet 启动，不需要可执行权限
	if strings.HasSuffix(strings.ToLower(path), ".dll") {
		return nil
	}
	return os.Chmod(path, 0755)
}