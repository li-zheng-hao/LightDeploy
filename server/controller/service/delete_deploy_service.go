package service

import (
	"ld_server/db"
	"ld_server/model"
	"ld_shared/error_response"

	"github.com/gofiber/fiber/v2"
)

// 删除部署服务
func DeleteDeployService(c *fiber.Ctx) error {
	id := c.Params("id")
	if id == "" {
		return error_response.NewErrorResponse(c, "id is required")
	}

	var service model.DeployService
	if err := db.DB.First(&service, id).Error; err != nil {
		return error_response.NewErrorResponse(c, "服务不存在")
	}

	if err := db.DB.Delete(&service).Error; err != nil {
		return error_response.NewErrorResponse(c, err.Error())
	}

	return c.JSON(fiber.Map{
		"affected": 1,
	})
}