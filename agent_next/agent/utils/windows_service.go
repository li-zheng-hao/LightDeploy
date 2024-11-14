package utils

import (
	"fmt"
	"os/exec"
	"path/filepath"
	"strings"

	"golang.org/x/sys/windows/svc"
	"golang.org/x/sys/windows/svc/mgr"
	"lightdeploy.agentclient/models"
)

// ServiceIsExisted 检查服务是否存在
func ServiceIsExisted(serviceName string) bool {
	m, err := mgr.Connect()
	if err != nil {
		return false
	}
	defer m.Disconnect()

	s, err := m.OpenService(serviceName)
	if err != nil {
		return false
	}
	defer s.Close()
	return true
}

// StartService 启动服务
func StartService(serviceName string) error {
	m, err := mgr.Connect()
	if err != nil {
		return fmt.Errorf("连接服务管理器失败: %v", err)
	}
	defer m.Disconnect()

	s, err := m.OpenService(serviceName)
	if err != nil {
		return fmt.Errorf("打开服务失败: %v", err)
	}
	defer s.Close()

	return s.Start()
}

// StopService 停止服务
func StopService(serviceName string) error {
	m, err := mgr.Connect()
	if err != nil {
		return fmt.Errorf("连接服务管理器失败: %v", err)
	}
	defer m.Disconnect()

	// 检查服务是否停止了
	if GetStatus(serviceName) == "Stopped" {
		return nil
	}

	s, err := m.OpenService(serviceName)
	if err != nil {
		return fmt.Errorf("打开服务失败: %v", err)
	}
	defer s.Close()

	_, err = s.Control(svc.Stop)
	return err
}

// GetStatus 获取服务状态
func GetStatus(serviceName string) string {
	m, err := mgr.Connect()
	if err != nil {
		return "Unknown"
	}
	defer m.Disconnect()

	s, err := m.OpenService(serviceName)
	if err != nil {
		return "Unknown"
	}
	defer s.Close()

	status, err := s.Query()
	if err != nil {
		return "Unknown"
	}

	switch status.State {
	case svc.Running:
		return "Running"
	case svc.Stopped:
		return "Stopped"
	case svc.StartPending:
		return "Starting"
	case svc.StopPending:
		return "Stopping"
	default:
		return "Unknown"
	}
}

// GetWindowsServiceLocation 查询指定服务的执行路径
func GetWindowsServiceLocation(serviceName string) (string, error) {
	m, err := mgr.Connect()
	if err != nil {
		return "", fmt.Errorf("连接服务管理器失败: %v", err)
	}
	defer m.Disconnect()

	s, err := m.OpenService(serviceName)
	if err != nil {
		return "", fmt.Errorf("打开服务失败: %v", err)
	}
	defer s.Close()

	// 获取服务配置信息
	config, err := s.Config()
	if err != nil {
		return "", fmt.Errorf("获取服务配置失败: %v", err)
	}

	path := config.BinaryPathName
	if strings.Contains(path, "nssm") {
		// 执行 nssm get 命令获取实际应用程序路径
		cmd := exec.Command("D:/nssm.exe", "get", serviceName, "Application")
		output, err := cmd.CombinedOutput()
		if err != nil {
			return "", fmt.Errorf("获取 nssm 服务实际路径失败: %v", err)
		}
		path = strings.TrimSpace(string(output))
		if path == "" {
			return "", fmt.Errorf("未能获取到服务的实际路径")
		}
	}
	// 获取文件夹
	dir := filepath.Dir(path)
	return dir, nil
}

func InstallWindowsService(dto models.InstallWindowsServiceDto) error {
	// 检查服务是否已存在
	if ServiceIsExisted(dto.ServiceName) {
		return fmt.Errorf("服务 '%s' 已存在", dto.ServiceName)
	}

	// 构建nssm命令参数
	nssmPath := "D:/nssm.exe"
	args := []string{
		"install",
		dto.ServiceName, // 服务名称
		dto.ExeFullPath, // 可执行文件路径
	}

	// 如果有运行参数，添加到命令中
	if dto.Params != "" {
		args = append(args, dto.Params)
	}

	// 执行安装命令
	cmd := exec.Command(nssmPath, args...)
	output, err := cmd.CombinedOutput()
	if err != nil {
		return fmt.Errorf("安装服务失败: %v, 输出: %s", err, string(output))
	}

	// 如果有服务描述，设置描述
	if dto.ServiceDescription != "" {
		descCmd := exec.Command(nssmPath, "set", dto.ServiceName, "Description", dto.ServiceDescription)
		if output, err := descCmd.CombinedOutput(); err != nil {
			return fmt.Errorf("设置服务描述失败: %v, 输出: %s", err, string(output))
		}
	}

	return nil
}
