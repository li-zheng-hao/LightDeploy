package service

import (
	"net/http"

	"ld_server/db"
	"ld_server/model"
	"ld_server/service"
	"ld_shared/error_response"

	"github.com/gin-gonic/gin"
)

func GetServiceStatus(c *gin.Context) {
	serviceId := c.Param("serviceId")
	if serviceId == "" {
		error_response.NewErrorResponse(c, "serviceId is required")
		return
	}

	serviceModel := new(model.DeployService)
	_, err := db.Engine.ID(serviceId).Get(serviceModel)
	if err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	// 获取服务关联的目标机器
	var targets []model.DeployTarget
	err = db.Engine.Where("service_id = ?", serviceId).Find(&targets)
	if err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	if len(targets) == 0 {
		error_response.NewErrorResponse(c, "no targets found for this service")
		return
	}

	results, err := service.GetServiceStatus(int(serviceModel.Id))
	if err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	c.JSON(http.StatusOK, results)
}

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
	affected, err := db.Engine.Insert(&service)
	if err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"affected": affected,
		"data":     service,
	})
}

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
	affected, err := db.Engine.ID(service.Id).UseBool("only_copy_file").Update(&service)
	if err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"affected": affected,
	})
}

// 获取所有部署服务
func GetDeployServices(c *gin.Context) {
	var services []model.DeployService
	err := db.Engine.Find(&services)
	if err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	c.JSON(http.StatusOK,services)
}

// 删除部署服务
func DeleteDeployService(c *gin.Context) {
	id := c.Param("id")
	if id == "" {
		error_response.NewErrorResponse(c, "id is required")
		return
	}

	service := new(model.DeployService)
	affected, err := db.Engine.ID(id).Delete(service)
	if err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"affected": affected,
	})
}