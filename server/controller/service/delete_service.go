package service

import (
	"fmt"
	"io"
	"net/http"

	"ld_server/db"
	"ld_server/model"
	"ld_shared/error_response"

	"github.com/gofiber/fiber/v2"
)

type DeleteServiceRequest struct {
	ServiceName string `json:"serviceName"`
	TargetIds   []int  `json:"targetIds"`
}

// 删除Windows服务接口
func DeleteService(c *fiber.Ctx) error {
	var request DeleteServiceRequest
	if err := c.BodyParser(&request); err != nil {
		return error_response.NewErrorResponse(c, err.Error())
	}

	serviceName := request.ServiceName
	if serviceName == "" {
		return error_response.NewErrorResponse(c, "serviceName is required")
	}
	if len(request.TargetIds) == 0 {
		return error_response.NewErrorResponse(c, "targetIds is required")
	}

	// 查询目标机器
	var targets []model.DeployTarget
	if err := db.DB.Where("id IN ?", request.TargetIds).Find(&targets).Error; err != nil {
		return error_response.NewErrorResponse(c, "查询目标机器失败: "+err.Error())
	}
	if len(targets) == 0 {
		return error_response.NewErrorResponse(c, "未找到目标机器")
	}

	for _, target := range targets {
		url := fmt.Sprintf("http://%s:%d/api/service/delete-service?serviceName=%s", target.Host, target.Port, serviceName)
		resp, err := http.Post(url, "application/json", nil)
		if err != nil {
			return error_response.NewErrorResponse(c, fmt.Sprintf("请求目标机器 %s 失败: %v", target.Host, err))
		}
		defer resp.Body.Close()
		if resp.StatusCode != http.StatusOK {
			bodyBytes, _ := io.ReadAll(resp.Body)
			return error_response.NewErrorResponse(c, fmt.Sprintf("目标机器 %s 删除服务失败: %s", target.Host, string(bodyBytes)))
		}
	}

	return c.JSON(fiber.Map{
		"message": "服务删除成功",
	})
}