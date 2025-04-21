package windows_service

import (
	"fmt"
	"log/slog"
	"strings"
	"time"

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

	// 将 exeParams 按空格拆分成数组
	params := strings.Fields(exeParams)

	// 创建服务
	s, err := m.CreateService(serviceName, exePath, mgr.Config{
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

func GetServiceStatus(serviceName string) (*svc.State, error) {
	// 连接服务管理器
	m, err := mgr.Connect()
	if err != nil {
		return nil, err
	}
	defer m.Disconnect()

	// 打开指定服务
	s, err := m.OpenService(serviceName)
	if err != nil {
		return nil, err
	}
	defer s.Close()
	status, err := s.Query()
	if err != nil {
		return nil, err
	}
	return &status.State, nil
}

func DeleteService(serviceName string) error {
	m, err := mgr.Connect()
	if err != nil {
		return err
	}
	defer m.Disconnect()

	s, err := m.OpenService(serviceName)
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
	err = s.Delete()
	if err != nil {
		return err
	}
	slog.Info("服务删除成功", "service", serviceName)
	return nil
}
