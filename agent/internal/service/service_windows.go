//go:build windows

package service

import (
	"fmt"
	"log/slog"
	"strings"
	"time"

	"golang.org/x/sys/windows/svc"
	"golang.org/x/sys/windows/svc/mgr"
)

type windowsManager struct{}

func newManager() Manager {
	return &windowsManager{}
}

// Install 使用 SCM 安装 Windows 服务
func (m *windowsManager) Install(serviceName, exePath, exeParams, workDir string) error {
	c, err := mgr.Connect()
	if err != nil {
		return err
	}
	defer c.Disconnect()

	slog.Info("安装服务", "exePath", exePath)

	// 将 exeParams 按空格拆分成数组
	params := strings.Fields(exeParams)

	// 创建服务
	s, err := c.CreateService(serviceName, exePath, mgr.Config{
		StartType:        mgr.StartAutomatic,
		DelayedAutoStart: true,
		DisplayName:      serviceName,                     // 显示名称
		Description:      "Installed by ld_agent service", // 服务描述
	}, params...)
	if err != nil {
		return err
	}
	defer s.Close()

	return nil
}

// Start 启动 Windows 服务
func (m *windowsManager) Start(serviceName string) error {
	c, err := mgr.Connect()
	if err != nil {
		return err
	}
	defer c.Disconnect()

	s, err := c.OpenService(serviceName)
	if err != nil {
		return err
	}
	defer s.Close()

	return s.Start()
}

// Stop 停止 Windows 服务
func (m *windowsManager) Stop(serviceName string) error {
	c, err := mgr.Connect()
	if err != nil {
		return err
	}
	defer c.Disconnect()

	s, err := c.OpenService(serviceName)
	if err != nil {
		return err
	}
	defer s.Close()

	_, err = s.Control(svc.Stop)
	return err
}

// Delete 删除 Windows 服务
func (m *windowsManager) Delete(serviceName string) error {
	c, err := mgr.Connect()
	if err != nil {
		return err
	}
	defer c.Disconnect()

	s, err := c.OpenService(serviceName)
	if err != nil {
		return err
	}
	defer s.Close()

	// 查询服务状态
	status, err := s.Query()
	if err != nil {
		return err
	}

	if status.State == svc.Running || status.State == svc.StartPending {
		slog.Info("服务正在运行，尝试停止", "service", serviceName)
		_, err := s.Control(svc.Stop)
		if err != nil {
			return err
		}
		// 等待服务停止
		for i := 0; i < 30; i++ { // 最多等30秒
			status, err = s.Query()
			if err != nil {
				return err
			}
			if status.State == svc.Stopped {
				break
			}
			time.Sleep(1 * time.Second)
		}
		if status.State != svc.Stopped {
			return fmt.Errorf("服务停止超时: %s", serviceName)
		}
	}
	time.Sleep(1 * time.Second)
	// 删除服务
	if err := s.Delete(); err != nil {
		return err
	}
	slog.Info("服务删除成功", "service", serviceName)
	return nil
}

// IsRunning 判断 Windows 服务是否正在运行
func (m *windowsManager) IsRunning(serviceName string) (bool, error) {
	state, err := queryState(serviceName)
	if err != nil {
		return false, err
	}
	return state == svc.Running, nil
}

// GetStatusText 获取 Windows 服务的可读状态
func (m *windowsManager) GetStatusText(serviceName string) (string, error) {
	state, err := queryState(serviceName)
	if err != nil {
		return "", err
	}
	return windowsStatusText(state), nil
}

// queryState 查询 Windows 服务的原始状态
func queryState(serviceName string) (svc.State, error) {
	c, err := mgr.Connect()
	if err != nil {
		return 0, err
	}
	defer c.Disconnect()

	s, err := c.OpenService(serviceName)
	if err != nil {
		return 0, err
	}
	defer s.Close()

	status, err := s.Query()
	if err != nil {
		return 0, err
	}
	return status.State, nil
}

// windowsStatusText 将 Windows 服务状态码转换为可读文本
func windowsStatusText(state svc.State) string {
	switch state {
	case svc.Stopped:
		return "已停止"
	case svc.StartPending:
		return "正在启动"
	case svc.StopPending:
		return "正在停止"
	case svc.Running:
		return "正在运行"
	case svc.ContinuePending:
		return "正在继续"
	case svc.PausePending:
		return "正在暂停"
	case svc.Paused:
		return "已暂停"
	default:
		return "未知状态"
	}
}

// EnsureExecutable Windows 下可执行权限由扩展名决定，无需处理
func EnsureExecutable(path string) error {
	return nil
}