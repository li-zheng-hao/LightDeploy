package deploy

import (
	"fmt"
	"ld_server/service/deploy"
	"ld_shared/error_response"
	sse_utils "ld_shared/sse"
	"net/http"

	"github.com/gin-gonic/gin"
)

func StopService(c *gin.Context) {
	var request ServiceOperationRequest
	if err := c.ShouldBindJSON(&request); err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	deployService, targets, err := deploy.ValidateDeployRequest(request.ServiceId, request.TargetIds)
	if err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	// 在所有目标机器上停止服务
	for _, target := range *targets {
		sse_utils.SendMessage(fmt.Sprintf("正在停止机器 %s 上的服务", target.Host))
		if err := deploy.StopServiceOnTarget(&target, deployService); err != nil {
			error_response.NewErrorResponse(c, err.Error())
			return
		}
	}

	c.JSON(http.StatusOK, gin.H{
		"message": "服务停止成功",
	})
}
