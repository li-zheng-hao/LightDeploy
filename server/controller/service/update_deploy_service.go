package service

import (
	"ld_server/db"
	"ld_server/model"
	"ld_shared/error_response"

	"github.com/gofiber/fiber/v2"
)

// 更新部署服务
func UpdateDeployService(c *fiber.Ctx) error {
	var service model.DeployService
	if err := c.BodyParser(&service); err != nil {
		return error_response.NewErrorResponse(c, err.Error())
	}

	if service.Id <= 0 {
		return error_response.NewErrorResponse(c, "id is required")
	}

	// 调用数据库更新
	if err := db.DB.Save(&service).Error; err != nil {
		return error_response.NewErrorResponse(c, err.Error())
	}

	return c.JSON(fiber.Map{
		"affected": 1,
	})
}