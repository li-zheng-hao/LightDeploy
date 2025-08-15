package service

import (
	"net/http"

	"ld_server/db"
	"ld_server/model"
	"ld_shared/error_response"

	"github.com/gin-gonic/gin"
)

// 更新部署服务
func UpdateDeployService(c *gin.Context) {
	var service model.DeployService
	if err := c.ShouldBindJSON(&service); err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	if service.Id <= 0 {
		error_response.NewErrorResponse(c, "id is required")
		return
	}

	// 调用数据库更新
	if err := db.DB.Save(&service).Error; err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"affected": 1,
	})
}
