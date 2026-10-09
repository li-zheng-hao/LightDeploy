package deploy

import (
	"ld_agent/internal/service"
	"ld_shared/error_response"

	"github.com/gofiber/fiber/v2"
)

// 删除服务接口（Windows/Linux 通用）
func DeleteService(c *fiber.Ctx) error {
	serviceName := c.Query("serviceName")
	if serviceName == "" {
		return error_response.NewErrorResponse(c, "serviceName is required")
	}

	err := service.NewManager().Delete(serviceName)
	if err != nil {
		return error_response.NewErrorResponse(c, "删除服务失败: "+err.Error())
	}

	return c.JSON(fiber.Map{
		"message": "服务删除成功",
	})
}