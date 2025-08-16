package target

import (
	"ld_server/db"
	"ld_server/model"
	"ld_shared/error_response"

	"github.com/gofiber/fiber/v2"
)

// DeleteTarget 删除部署目标
func DeleteTarget(c *fiber.Ctx) error {
	id := c.Params("id")

	// 验证记录是否存在
	var target model.DeployTarget
	if err := db.DB.First(&target, id).Error; err != nil {
		return error_response.NewErrorResponse(c, "部署目标不存在")
	}

	// 删除记录
	if err := db.DB.Delete(&target).Error; err != nil {
		return error_response.NewErrorResponse(c, "删除部署目标失败")
	}

	return c.JSON(SuccessResponse{Message: "删除成功"})
}