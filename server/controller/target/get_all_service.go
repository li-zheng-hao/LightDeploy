package target

import (
	"ld_server/db"
	"ld_shared/error_response"

	"github.com/gofiber/fiber/v2"
)

// GetAllServiceResponse 获取所有服务的响应结构
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
func GetAllService(c *fiber.Ctx) error {
	host := c.Query("host")

	var results []GetAllServiceResponse

	if host != "" {
		// 查询指定主机上的所有服务
		err := db.DB.Table("deploy_target").
			Joins("LEFT JOIN deploy_service ON deploy_target.service_id = deploy_service.id").
			Where("deploy_target.host = ?", host).
			Select("deploy_target.host, deploy_service.service_name, deploy_service.environment, deploy_service.group_name, deploy_service.port").
			Find(&results).Error

		if err != nil {
			return error_response.NewErrorResponse(c, "获取服务列表失败: "+err.Error())
		}
	} else {
		// 查询所有主机上的所有服务
		err := db.DB.Table("deploy_target").
			Joins("LEFT JOIN deploy_service ON deploy_target.service_id = deploy_service.id").
			Select("deploy_target.host, deploy_service.service_name, deploy_service.environment, deploy_service.group_name, deploy_service.port").
			Find(&results).Error

		if err != nil {
			return error_response.NewErrorResponse(c, "获取服务列表失败: "+err.Error())
		}
	}

	return c.JSON(results)
}