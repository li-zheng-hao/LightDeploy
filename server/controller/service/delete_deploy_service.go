package service

import (
	"net/http"

	"ld_server/db"
	"ld_server/model"
	"ld_shared/error_response"

	"github.com/gin-gonic/gin"
)

// 删除部署服务
func DeleteDeployService(c *gin.Context) {
	id := c.Param("id")
	if id == "" {
		error_response.NewErrorResponse(c, "id is required")
		return
	}

	var service model.DeployService
	if err := db.DB.First(&service, id).Error; err != nil {
		error_response.NewErrorResponse(c, "服务不存在")
		return
	}

	if err := db.DB.Delete(&service).Error; err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"affected": 1,
	})
}
