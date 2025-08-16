package service

import (
	"ld_server/db"
	"ld_server/model"
	"ld_server/service/target"
	"ld_shared/error_response"

	"github.com/gofiber/fiber/v2"
)

func GetServiceStatus(c *fiber.Ctx) error {
	serviceId := c.Params("serviceId")
	if serviceId == "" {
		return error_response.NewErrorResponse(c, "serviceId is required")
	}

	var serviceModel model.DeployService
	if err := db.DB.First(&serviceModel, serviceId).Error; err != nil {
		return error_response.NewErrorResponse(c, err.Error())
	}

	// 获取服务关联的目标机器
	var targets []model.DeployTarget
	if err := db.DB.Where("service_id = ?", serviceId).Find(&targets).Error; err != nil {
		return error_response.NewErrorResponse(c, err.Error())
	}

	if len(targets) == 0 {
		return error_response.NewErrorResponse(c, "no targets found for this service")
	}

	results, err := target.GetServiceStatus(int(serviceModel.Id))
	if err != nil {
		return error_response.NewErrorResponse(c, err.Error())
	}

	return c.JSON(results)
}