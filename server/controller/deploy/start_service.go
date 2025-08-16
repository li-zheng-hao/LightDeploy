package deploy

import (
	"fmt"
	"ld_server/service/deploy"
	"ld_shared/error_response"
	sse_utils "ld_shared/sse"

	"github.com/gofiber/fiber/v2"
)

type ServiceOperationRequest struct {
	ServiceId int   `json:"serviceId"`
	TargetIds []int `json:"targetIds"`
}

func StartService(c *fiber.Ctx) error {
	var request ServiceOperationRequest
	if err := c.BodyParser(&request); err != nil {
		return error_response.NewErrorResponse(c, err.Error())
	}

	deployService, targets, err := deploy.ValidateDeployRequest(request.ServiceId, request.TargetIds)
	if err != nil {
		return error_response.NewErrorResponse(c, err.Error())
	}

	// 在所有目标机器上启动服务
	for _, target := range *targets {
		sse_utils.SendMessage(fmt.Sprintf("正在启动机器 %s 上的服务", target.Host))
		if err := deploy.StartServiceOnTarget(&target, deployService); err != nil {
			return error_response.NewErrorResponse(c, err.Error())
		}
	}

	return c.JSON(fiber.Map{
		"message": "服务启动成功",
	})
}