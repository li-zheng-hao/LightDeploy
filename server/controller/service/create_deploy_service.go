package service

import (
	"net/http"

	"ld_server/db"
	"ld_server/model"
	"ld_shared/error_response"

	"github.com/gin-gonic/gin"
)

// 创建部署服务
func CreateDeployService(c *gin.Context) {
	var service model.DeployService
	if err := c.ShouldBindJSON(&service); err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	// 强制设置id为0，确保使用数据库自增id
	service.Id = 0

	// 调用数据库创建
	if err := db.DB.Create(&service).Error; err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"affected": 1,
		"data":     service,
	})
}
