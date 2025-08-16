package deploy

import (
	"ld_agent/internal/windows_service"
	"ld_shared/error_response"

	"github.com/gofiber/fiber/v2"
)

// 删除Windows服务接口
func DeleteService(c *fiber.Ctx) error {
	serviceName := c.Query("serviceName")
	if serviceName == "" {
		return error_response.NewErrorResponse(c, "serviceName is required")
	}

	err := windows_service.DeleteService(serviceName)
	if err != nil {
		return error_response.NewErrorResponse(c, "删除服务失败: "+err.Error())
	}

	return c.JSON(fiber.Map{
		"message": "服务删除成功",
	})
}