package service

import (
	"ld_server/db"
	"ld_server/model"
	"ld_shared/error_response"

	"github.com/gofiber/fiber/v2"
)

// 创建部署服务
func CreateDeployService(c *fiber.Ctx) error {
	var service model.DeployService
	if err := c.BodyParser(&service); err != nil {
		return error_response.NewErrorResponse(c, err.Error())
	}

	// 强制设置id为0，确保使用数据库自增id
	service.Id = 0

	// 调用数据库创建
	if err := db.DB.Create(&service).Error; err != nil {
		return error_response.NewErrorResponse(c, err.Error())
	}

	return c.JSON(fiber.Map{
		"affected": 1,
		"data":     service,
	})
}