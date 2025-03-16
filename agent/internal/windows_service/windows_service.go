package windows_service

import (
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

func InstallService(serviceName string, servicePath string, description string) error {
	// 连接服务管理器	
	m, err := mgr.Connect()
	if err != nil {
		return err
	}
	defer m.Disconnect()

	// 创建服务
	s, err := m.CreateService(serviceName, servicePath, mgr.Config{
		StartType: mgr.StartAutomatic,
		Description: description,
		DelayedAutoStart: true,
	})
	if err != nil {
		return err
	}
	defer s.Close()

	return nil
}

