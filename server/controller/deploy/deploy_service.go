package deploy

import (
	"fmt"
	"ld_server/service/deploy"
	"ld_server/service/notify"
	"ld_shared/error_response"
	"os"

	sse_utils "ld_shared/sse"

	"github.com/gofiber/fiber/v2"
)

type DeployServiceRequest struct {
	ServiceId int    `json:"serviceId"`
	TargetIds []int  `json:"targetIds"`
	Comment   string `json:"comment"`
	// 快速模式，元数据初筛后按需校验文件内容
	UseFastMode bool `json:"useFastMode"`
	// 是否发送企业微信通知
	SendWecomNotify bool `json:"sendWecomNotify"`
}

func DeployService(c *fiber.Ctx) error {
	var request DeployServiceRequest
	if err := c.BodyParser(&request); err != nil {
		return error_response.NewErrorResponse(c, err.Error())
	}
	deployService, targets, err := deploy.ValidateDeployRequest(request.ServiceId, request.TargetIds)
	if err != nil {
		return error_response.NewErrorResponse(c, err.Error())
	}

	// 提前收集部署目标主机列表，用于企业微信通知
	hosts := make([]string, 0, len(*targets))
	for _, target := range *targets {
		hosts = append(hosts, fmt.Sprintf("%s:%d", target.Host, target.Port))
	}

	// 执行部署，捕获成功/失败结果，以便发送通知
	deployErr := func() error {
		if request.UseFastMode {
			sse_utils.SendMessage("开始快速模式部署")
			// 所有目标共用一次目录扫描和源文件哈希缓存。
			source, err := deploy.NewFastDeploySource(deployService.ProjectPath, deployService.IgnoreFileRegex)
			if err != nil {
				return err
			}

			// 快速模式部署到所有目标机器
			for _, target := range *targets {
				sse_utils.SendMessage(fmt.Sprintf("开始处理目标机器: %s", target.Host))
				if err := source.DeployToTarget(&target, deployService); err != nil {
					return err
				}
			}
			return nil
		}

		tempDir, zipFilePath, err := deploy.PrepareDeployPackage(deployService)
		if err != nil {
			return err
		}
		defer os.RemoveAll(tempDir)

		// 部署到所有目标机器
		for _, target := range *targets {
			if err := deploy.DeployToTarget(target, deployService, zipFilePath); err != nil {
				return err
			}
		}
		return nil
	}()

	// 勾选了通知则发送企业微信结果通知（失败不阻断部署流程）
	if request.SendWecomNotify {
		_ = notify.SendDeployNotification(notify.DeployNotifyInfo{
			WebhookKey:   deployService.WecomWebhookKey,
			Environment:  deployService.Environment,
			GroupName:    deployService.GroupName,
			ServiceName:  deployService.ServiceName,
			Hosts:        hosts,
			Success:      deployErr == nil,
			Comment:      request.Comment,
			MentionUsers: deployService.WecomNotifyUsers,
		})
	}

	if deployErr != nil {
		return error_response.NewErrorResponse(c, deployErr.Error())
	}

	// 保存部署历史
	if err := deploy.SaveDeployHistory(deployService.Id, request.Comment); err != nil {
		return error_response.NewErrorResponse(c, err.Error())
	}

	return c.JSON(fiber.Map{
		"message": "部署成功",
	})
}
