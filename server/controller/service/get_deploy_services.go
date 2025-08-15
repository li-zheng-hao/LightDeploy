package service

import (
	"net/http"

	"ld_server/db"
	"ld_server/model"
	"ld_shared/error_response"

	"github.com/gin-gonic/gin"
)

// 获取所有部署服务
func GetDeployServices(c *gin.Context) {
	var services []model.DeployService
	if err := db.DB.Find(&services).Error; err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	c.JSON(http.StatusOK, services)
}
