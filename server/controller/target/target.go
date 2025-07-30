package target

import (
	"ld_server/db"
	"ld_server/model"
	"ld_shared/error_response"
	"net/http"

	"github.com/gin-gonic/gin"
)

// CreateTarget 创建部署目标
func CreateTarget(c *gin.Context) {
	var target model.DeployTarget
	if err := c.ShouldBindJSON(&target); err != nil {
		error_response.NewErrorResponse(c, "参数无效")
		return
	}

	// 验证必填字段
	if target.ServiceId == 0 || target.Host == "" || target.Port == 0 {
		error_response.NewErrorResponse(c, "服务ID、主机地址和端口为必填项")
		return
	}

	// 插入数据库
	_, err := db.Engine.Insert(&target)
	if err != nil {
		error_response.NewErrorResponse(c, "创建部署目标失败,"+err.Error())
		return
	}

	c.JSON(http.StatusOK, target)
}

// GetTarget 获取单个部署目标
func GetTarget(c *gin.Context) {
	id := c.Param("id")
	var target model.DeployTarget
	has, err := db.Engine.ID(id).Get(&target)

	if err != nil {
		error_response.NewErrorResponse(c, "获取部署目标失败")
		return
	}
	if !has {
		error_response.NewErrorResponse(c, "部署目标不存在")
		return
	}

	c.JSON(http.StatusOK, target)
}

// ListTargets 获取部署目标列表
func ListTargets(c *gin.Context) {
	serviceId := c.Query("serviceId")
	var targets []model.DeployTarget
	if serviceId != "" {
		err := db.Engine.Where("service_id = ?", serviceId).Find(&targets)
		if err != nil {
			error_response.NewErrorResponse(c, "获取部署目标列表失败")
			return
		}
	} else {
		err := db.Engine.Find(&targets)
		if err != nil {
			error_response.NewErrorResponse(c, "获取部署目标列表失败")
			return
		}
	}

	c.JSON(http.StatusOK, targets)
}

// UpdateTarget 更新部署目标
func UpdateTarget(c *gin.Context) {
	var target model.DeployTarget
	if err := c.ShouldBindJSON(&target); err != nil {
		error_response.NewErrorResponse(c, "参数无效")
		return
	}

	// 验证记录是否存在
	exists, err := db.Engine.ID(target.Id).Exist(&model.DeployTarget{})
	if err != nil {
		error_response.NewErrorResponse(c, "系统错误")
		return
	}
	if !exists {
		error_response.NewErrorResponse(c, "部署目标不存在")
		return
	}

	// 更新记录
	_, err = db.Engine.ID(target.Id).Update(&target)
	if err != nil {
		error_response.NewErrorResponse(c, "更新部署目标失败")
		return
	}

	c.JSON(http.StatusOK, gin.H{"message": "更新成功"})
}

// DeleteTarget 删除部署目标
func DeleteTarget(c *gin.Context) {
	id := c.Param("id")

	// 验证记录是否存在
	exists, err := db.Engine.ID(id).Exist(&model.DeployTarget{})
	if err != nil {
		error_response.NewErrorResponse(c, "系统错误")
		return
	}
	if !exists {
		error_response.NewErrorResponse(c, "部署目标不存在")
		return
	}

	// 删除记录
	_, err = db.Engine.ID(id).Delete(&model.DeployTarget{})
	if err != nil {
		error_response.NewErrorResponse(c, "删除部署目标失败")
		return
	}

	c.JSON(http.StatusOK, gin.H{"message": "删除成功"})
}

type GetAllServiceResponse struct {
	Host        string `json:"host"`
	ServiceName string `json:"serviceName"`
	Environment string `json:"environment"`
	GroupName   string `json:"groupName"`
	Port        int    `json:"port"`
}

/*
获取指定目标主机上的所有服务，没传目标主机的话查询所有主机上的所有服务
*/
func GetAllService(c *gin.Context) {
	host := c.Query("host")

	var results []GetAllServiceResponse

	if host != "" {
		// 查询指定主机上的所有服务
		err := db.Engine.Table("deploy_target").
			Join("LEFT", "deploy_service", "deploy_target.service_id = deploy_service.id").
			Where("deploy_target.host = ?", host).
			Select("deploy_target.host, deploy_service.service_name, deploy_service.environment, deploy_service.group_name, deploy_service.port").
			Find(&results)

		if err != nil {
			error_response.NewErrorResponse(c, "获取服务列表失败: "+err.Error())
			return
		}
	} else {
		// 查询所有主机上的所有服务
		err := db.Engine.Table("deploy_target").
			Join("LEFT", "deploy_service", "deploy_target.service_id = deploy_service.id").
			Select("deploy_target.host, deploy_service.service_name, deploy_service.environment, deploy_service.group_name, deploy_service.port").
			Find(&results)

		if err != nil {
			error_response.NewErrorResponse(c, "获取服务列表失败: "+err.Error())
			return
		}
	}

	c.JSON(http.StatusOK, results)
}

type GetAllHostsResponse struct {
	Host string `json:"host"`
}

/*
获取所有目标主机，去重
*/
func GetAllHosts(c *gin.Context) {
	var results []GetAllHostsResponse

	// 查询所有不重复的主机地址
	err := db.Engine.Table("deploy_target").
		Select("DISTINCT host").
		Find(&results)

	if err != nil {
		error_response.NewErrorResponse(c, "获取主机列表失败: "+err.Error())
		return
	}

	c.JSON(http.StatusOK, results)
}
