package metric

import (
	"ld_server/service/metric"
	"ld_shared/error_response"

	"github.com/gofiber/fiber/v2"
)

// GetOverview 仪表盘概览数据
func GetOverview(c *fiber.Ctx) error {
	data, err := metric.GetOverview()
	if err != nil {
		return error_response.NewErrorResponse(c, err.Error())
	}
	return c.JSON(data)
}