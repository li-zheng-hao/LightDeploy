package target

import (
	"ld_server/db"
	"ld_server/model"
	"ld_shared/error_response"

	"github.com/gofiber/fiber/v2"
)

// GetTarget 获取单个部署目标
func GetTarget(c *fiber.Ctx) error {
	id := c.Params("id")
	var target model.DeployTarget
	err := db.DB.First(&target, id).Error

	if err != nil {
		return error_response.NewErrorResponse(c, "获取部署目标失败")
	}
	if err != nil {
		return error_response.NewErrorResponse(c, "部署目标不存在")
	}

	return c.JSON(target)
}