package deploy

import (
	"fmt"
	"ld_server/model"
	"ld_server/service/deploy"
	"ld_shared/error_response"
	"ld_shared/process"
	"os"
	"path/filepath"

	sse_utils "ld_shared/sse"

	"github.com/gofiber/fiber/v2"
	"github.com/google/uuid"
)

type DeployServiceRequest struct {
	ServiceId int    `json:"serviceId"`
	TargetIds []int  `json:"targetIds"`
	Comment   string `json:"comment"`
	// 快速模式，仅对比文件大小和修改时间
	UseFastMode bool `json:"useFastMode"`
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
	if request.UseFastMode {
		sse_utils.SendMessage("开始快速模式部署")
		executablePath, err := os.Executable()
		if err != nil {
			return error_response.NewErrorResponse(c, err.Error())
		}
		tempDir := filepath.Join(filepath.Dir(executablePath), "ld_deploy_temp", uuid.New().String())
		if err := os.MkdirAll(tempDir, 0755); err != nil {
			return error_response.NewErrorResponse(c, err.Error())
		}
		defer os.RemoveAll(tempDir)

		if deployService.ProjectType == model.ProjectTypeNetCore {
			sse_utils.SendMessage("开始构建项目")
			process.ExecuteWindowsCommand("dotnet publish " + deployService.ProjectPath + " -o " + tempDir)
			sse_utils.SendMessage("构建完成")
		} else {
			tempDir = deployService.ProjectPath
		}

		// 快速模式部署到所有目标机器
		for _, target := range *targets {
			sse_utils.SendMessage(fmt.Sprintf("开始处理目标机器: %s", target.Host))
			if err := deploy.FastDeployToTarget(&target, deployService, tempDir); err != nil {
				return error_response.NewErrorResponse(c, err.Error())
			}
		}
	} else {
		tempDir, zipFilePath, err := deploy.PrepareDeployPackage(deployService)
		if err != nil {
			return error_response.NewErrorResponse(c, err.Error())
		}
		defer os.RemoveAll(tempDir)

		// 部署到所有目标机器
		for _, target := range *targets {
			if err := deploy.DeployToTarget(target, deployService, zipFilePath); err != nil {
				return error_response.NewErrorResponse(c, err.Error())
			}
		}
	}

	// 保存部署历史
	if err := deploy.SaveDeployHistory(deployService.Id, request.Comment); err != nil {
		return error_response.NewErrorResponse(c, err.Error())
	}

	return c.JSON(fiber.Map{
		"message": "部署成功",
	})
}