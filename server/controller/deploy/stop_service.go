package deploy

import (
	"fmt"
	"ld_server/service/deploy"
	"ld_shared/error_response"
	sse_utils "ld_shared/sse"

	"github.com/gofiber/fiber/v2"
)

func StopService(c *fiber.Ctx) error {
	var request ServiceOperationRequest
	if err := c.BodyParser(&request); err != nil {
		return error_response.NewErrorResponse(c, err.Error())
	}

	deployService, targets, err := deploy.ValidateDeployRequest(request.ServiceId, request.TargetIds)
	if err != nil {
		return error_response.NewErrorResponse(c, err.Error())
	}

	// 在所有目标机器上停止服务
	for _, target := range *targets {
		sse_utils.SendMessage(fmt.Sprintf("正在停止机器 %s 上的服务", target.Host))
		if err := deploy.StopServiceOnTarget(&target, deployService); err != nil {
			return error_response.NewErrorResponse(c, err.Error())
		}
	}

	return c.JSON(fiber.Map{
		"message": "服务停止成功",
	})
}