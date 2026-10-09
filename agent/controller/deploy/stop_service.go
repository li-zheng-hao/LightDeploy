package deploy

import (
	"ld_agent/internal/service"
	"ld_shared/error_response"

	"log/slog"

	"github.com/gofiber/fiber/v2"
)

func StopService(c *fiber.Ctx) error {
	slog.Info("开始停止服务")
	serviceName := c.Query("serviceName")
	if serviceName == "" {
		return error_response.NewErrorResponse(c, "服务名称不能为空")
	}
	err := service.NewManager().Stop(serviceName)
	if err != nil {
		return error_response.NewErrorResponse(c, err.Error())
	}
	return c.JSON(nil)
}