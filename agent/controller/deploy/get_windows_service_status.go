package deploy

import (
	"ld_shared/error_response"

	"golang.org/x/sys/windows/svc"
	"golang.org/x/sys/windows/svc/mgr"

	"github.com/gofiber/fiber/v2"
)

func GetWindowsServiceStatus(c *fiber.Ctx) error {
	serviceName := c.Query("serviceName")
	if serviceName == "" {
		return error_response.NewErrorResponse(c, "serviceName is required")
	}

	// 连接服务管理器
	m, err := mgr.Connect()
	if err != nil {
		return error_response.NewErrorResponse(c, "无法连接服务管理器: "+err.Error())
	}
	defer m.Disconnect()

	// 打开指定服务
	s, err := m.OpenService(serviceName)
	if err != nil {
		return error_response.NewErrorResponse(c, "服务不存在: "+err.Error())
	}
	defer s.Close()

	// 查询服务状态
	status, err := s.Query()
	if err != nil {
		return error_response.NewErrorResponse(c, "无法查询服务状态: "+err.Error())
	}

	// 将状态码转换为可读的状态描述
	statusText := getStatusText(status.State)

	return c.JSON(fiber.Map{
		"serviceName": serviceName,
		"status":      statusText,
		"state":       status.State,
	})
}

// 将状态码转换为可读的文本
func getStatusText(state svc.State) string {
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