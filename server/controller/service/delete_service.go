package service

import (
	"fmt"
	"io"
	"net/http"

	"ld_server/db"
	"ld_server/model"
	"ld_shared/error_response"

	"github.com/gin-gonic/gin"
)

type DeleteServiceRequest struct {
	ServiceName string `json:"serviceName"`
	TargetIds   []int  `json:"targetIds"`
}

// 删除Windows服务接口
func DeleteService(c *gin.Context) {
	var request DeleteServiceRequest
	if err := c.ShouldBindJSON(&request); err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	serviceName := request.ServiceName
	if serviceName == "" {
		error_response.NewErrorResponse(c, "serviceName is required")
		return
	}
	if len(request.TargetIds) == 0 {
		error_response.NewErrorResponse(c, "targetIds is required")
		return
	}

	// 查询目标机器
	var targets []model.DeployTarget
	if err := db.DB.Where("id IN ?", request.TargetIds).Find(&targets).Error; err != nil {
		error_response.NewErrorResponse(c, "查询目标机器失败: "+err.Error())
		return
	}
	if len(targets) == 0 {
		error_response.NewErrorResponse(c, "未找到目标机器")
		return
	}

	for _, target := range targets {
		url := fmt.Sprintf("http://%s:%d/api/service/delete-service?serviceName=%s", target.Host, target.Port, serviceName)
		resp, err := http.Post(url, "application/json", nil)
		if err != nil {
			error_response.NewErrorResponse(c, fmt.Sprintf("请求目标机器 %s 失败: %v", target.Host, err))
			return
		}
		defer resp.Body.Close()
		if resp.StatusCode != http.StatusOK {
			bodyBytes, _ := io.ReadAll(resp.Body)
			error_response.NewErrorResponse(c, fmt.Sprintf("目标机器 %s 删除服务失败: %s", target.Host, string(bodyBytes)))
			return
		}
	}

	c.JSON(http.StatusOK, gin.H{
		"message": "服务删除成功",
	})
}
