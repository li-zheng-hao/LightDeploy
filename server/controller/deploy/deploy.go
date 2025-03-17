package deploy

import (
	"fmt"
	"ld_server/model"
	"ld_server/service/deploy"
	"ld_shared/error_response"
	"ld_shared/process"
	sse_utils "ld_shared/sse"
	"net/http"
	"os"
	"path/filepath"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type DeployServiceRequest struct {
	ServiceId int    `json:"serviceId"`
	TargetIds []int  `json:"targetIds"`
	Comment   string `json:"comment"`
	// 快速模式，仅对比文件大小和修改时间
	UseFastMode bool `json:"useFastMode"`
}

func DeployService(c *gin.Context) {
	var request DeployServiceRequest
	if err := c.ShouldBindJSON(&request); err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	deployService, targets, err := deploy.ValidateDeployRequest(request.ServiceId, request.TargetIds)
	if err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}
	if request.UseFastMode {
		sse_utils.SendMessage("开始快速模式部署")
		executablePath, err := os.Executable()
		if err != nil {
			error_response.NewErrorResponse(c, err.Error())
			return
		}
		tempDir := filepath.Join(filepath.Dir(executablePath), "ld_deploy_temp", uuid.New().String())
		if err := os.MkdirAll(tempDir, 0755); err != nil {
			error_response.NewErrorResponse(c, err.Error())
			return
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
				error_response.NewErrorResponse(c, err.Error())
				return
			}
		}
	} else {
		tempDir, zipFilePath, err := deploy.PrepareDeployPackage(deployService)
		if err != nil {
			error_response.NewErrorResponse(c, err.Error())
			return
		}
		defer os.RemoveAll(tempDir)

		// 部署到所有目标机器
		for _, target := range *targets {
			if err := deploy.DeployToTarget(target, deployService, zipFilePath); err != nil {
				error_response.NewErrorResponse(c, err.Error())
				return
			}
		}
	}

	// 保存部署历史
	if err := deploy.SaveDeployHistory(deployService.Id, request.Comment); err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"message": "部署成功",
	})
}

type ServiceOperationRequest struct {
	ServiceId int   `json:"serviceId"`
	TargetIds []int `json:"targetIds"`
}

func StartService(c *gin.Context) {
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

	// 在所有目标机器上启动服务
	for _, target := range *targets {
		sse_utils.SendMessage(fmt.Sprintf("正在启动机器 %s 上的服务", target.Host))
		if err := deploy.StartServiceOnTarget(&target, deployService); err != nil {
			error_response.NewErrorResponse(c, err.Error())
			return
		}
	}

	c.JSON(http.StatusOK, gin.H{
		"message": "服务启动成功",
	})
}

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
