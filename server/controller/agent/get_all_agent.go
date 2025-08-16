package agent

import (
	"ld_server/db"
	"ld_server/model"
	"ld_shared/error_response"

	"github.com/gofiber/fiber/v2"
)

func GetAllAgent(c *fiber.Ctx) error {
	// 从数据库中获取所有目标机器信息，按host去重
	var targets []model.DeployTarget
	if err := db.DB.Raw(`
		SELECT * FROM deploy_target
		WHERE id IN (
			SELECT MIN(id) 
			FROM deploy_target
			GROUP BY host
		)
	`).Scan(&targets).Error; err != nil {
		return error_response.NewErrorResponse(c, "获取目标机器列表失败: "+err.Error())
	}

	return c.JSON(targets)
}
