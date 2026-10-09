package deploy

import (
	"ld_agent/internal/service"
	"ld_shared/error_response"

	"github.com/gofiber/fiber/v2"
)

// GetWindowsServiceStatus 获取服务状态（Windows/Linux 通用）
func GetWindowsServiceStatus(c *fiber.Ctx) error {
	serviceName := c.Query("serviceName")
	if serviceName == "" {
		return error_response.NewErrorResponse(c, "serviceName is required")
	}

	statusText, err := service.NewManager().GetStatusText(serviceName)
	if err != nil {
		return error_response.NewErrorResponse(c, "无法查询服务状态: "+err.Error())
	}

	return c.JSON(fiber.Map{
		"serviceName": serviceName,
		"status":      statusText,
	})
}