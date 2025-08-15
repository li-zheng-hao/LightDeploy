package service

import (
	"net/http"

	"ld_server/db"
	"ld_server/model"
	"ld_server/service/target"
	"ld_shared/error_response"

	"github.com/gin-gonic/gin"
)

func GetServiceStatus(c *gin.Context) {
	serviceId := c.Param("serviceId")
	if serviceId == "" {
		error_response.NewErrorResponse(c, "serviceId is required")
		return
	}

	var serviceModel model.DeployService
	if err := db.DB.First(&serviceModel, serviceId).Error; err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	// 获取服务关联的目标机器
	var targets []model.DeployTarget
	if err := db.DB.Where("service_id = ?", serviceId).Find(&targets).Error; err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	if len(targets) == 0 {
		error_response.NewErrorResponse(c, "no targets found for this service")
		return
	}

	results, err := target.GetServiceStatus(int(serviceModel.Id))
	if err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	c.JSON(http.StatusOK, results)
}
