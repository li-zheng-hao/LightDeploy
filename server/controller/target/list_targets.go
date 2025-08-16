package target

import (
	"ld_server/db"
	"ld_server/model"
	"ld_shared/error_response"

	"github.com/gofiber/fiber/v2"
)

// ListTargets 获取部署目标列表
func ListTargets(c *fiber.Ctx) error {
	serviceId := c.Query("serviceId")
	var targets []model.DeployTarget
	if serviceId != "" {
		if err := db.DB.Where("service_id = ?", serviceId).Find(&targets).Error; err != nil {
			return error_response.NewErrorResponse(c, "获取部署目标列表失败")
		}
	} else {
		if err := db.DB.Find(&targets).Error; err != nil {
			return error_response.NewErrorResponse(c, "获取部署目标列表失败")
		}
	}

	return c.JSON(targets)
}