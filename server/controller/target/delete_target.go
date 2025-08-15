package target

import (
	"ld_server/db"
	"ld_server/model"
	"ld_shared/error_response"
	"net/http"

	"github.com/gin-gonic/gin"
)

// DeleteTarget 删除部署目标
func DeleteTarget(c *gin.Context) {
	id := c.Param("id")

	// 验证记录是否存在
	var target model.DeployTarget
	if err := db.DB.First(&target, id).Error; err != nil {
		error_response.NewErrorResponse(c, "部署目标不存在")
		return
	}

	// 删除记录
	if err := db.DB.Delete(&target).Error; err != nil {
		error_response.NewErrorResponse(c, "删除部署目标失败")
		return
	}

	c.JSON(http.StatusOK, SuccessResponse{Message: "删除成功"})
}
