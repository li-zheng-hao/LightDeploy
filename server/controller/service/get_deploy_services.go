package service

import (
	"ld_server/db"
	"ld_server/model"
	"ld_shared/error_response"

	"github.com/gofiber/fiber/v2"
)

// 获取所有部署服务
func GetDeployServices(c *fiber.Ctx) error {
	var services []model.DeployService
	if err := db.DB.Find(&services).Error; err != nil {
		return error_response.NewErrorResponse(c, err.Error())
	}

	return c.JSON(services)
}