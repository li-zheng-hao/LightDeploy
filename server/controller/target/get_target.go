package target

import (
	"ld_server/db"
	"ld_server/model"
	"ld_shared/error_response"
	"net/http"

	"github.com/gin-gonic/gin"
)

// GetTarget 获取单个部署目标
func GetTarget(c *gin.Context) {
	id := c.Param("id")
	var target model.DeployTarget
	err := db.DB.First(&target, id).Error

	if err != nil {
		error_response.NewErrorResponse(c, "获取部署目标失败")
		return
	}
	if err != nil {
		error_response.NewErrorResponse(c, "部署目标不存在")
		return
	}

	c.JSON(http.StatusOK, target)
}
