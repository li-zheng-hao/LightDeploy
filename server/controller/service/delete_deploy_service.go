package service

import (
	"ld_server/db"
	"ld_server/model"
	"ld_shared/error_response"

	"github.com/gofiber/fiber/v2"
	"gorm.io/gorm"
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

	// 在同一事务中删除服务及其关联的部署目标，避免残留孤儿数据
	if err := db.DB.Transaction(func(tx *gorm.DB) error {
		if err := tx.Where("service_id = ?", service.Id).Delete(&model.DeployTarget{}).Error; err != nil {
			return err
		}
		return tx.Delete(&service).Error
	}); err != nil {
		return error_response.NewErrorResponse(c, err.Error())
	}

	return c.JSON(fiber.Map{
		"affected": 1,
	})
}