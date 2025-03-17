package windows_service

import (
	"log/slog"

	"golang.org/x/sys/windows/svc"
	"golang.org/x/sys/windows/svc/mgr"
)

func StartService(serviceName string) error {
	// 连接服务管理器
	m, err := mgr.Connect()
	if err != nil {
		return err
	}
	defer m.Disconnect()

	// 打开指定服务
	s, err := m.OpenService(serviceName)
	if err != nil {
		return err
	}
	defer s.Close()

	// 启动服务
	err = s.Start()
	if err != nil {
		return err
	}
	return nil
}

func StopService(serviceName string) error {
	// 连接服务管理器
	m, err := mgr.Connect()
	if err != nil {
		return err
	}
	defer m.Disconnect()

	// 打开指定服务
	s, err := m.OpenService(serviceName)
	if err != nil {
		return err
	}
	defer s.Close()

	// 停止服务
	_, err = s.Control(svc.Stop)
	if err != nil {
		return err
	}
	return nil
}

func InstallService(serviceName string, exePath string, exeParams string) error {
	// 连接服务管理器
	m, err := mgr.Connect()
	if err != nil {
		return err
	}
	defer m.Disconnect()
	slog.Info("安装服务", "exePath", exePath)
	// 创建服务
	s, err := m.CreateService(serviceName, exePath, mgr.Config{
		StartType:        mgr.StartAutomatic,
		DelayedAutoStart: true,
		DisplayName:      serviceName,                     // 显示名称
		Description:      "Installed by ld_agent service", // 服务描述
	}, exeParams)
	if err != nil {
		return err
	}
	defer s.Close()

	return nil
}
