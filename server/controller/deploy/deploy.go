package deploy

import (
	"ld_server/service"
	"ld_shared/error_response"
	"net/http"
	"os"

	"github.com/gin-gonic/gin"
)

type DeployServiceRequest struct {
	ServiceId int    `json:"serviceId"`
	TargetIds []int  `json:"targetIds"`
	Comment  string `json:"comment"`
}

func DeployService(c *gin.Context) {
	var request DeployServiceRequest
	if err := c.ShouldBindJSON(&request); err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	deployService := service.NewDeployService()

	service, targets, err := deployService.ValidateDeployRequest(request.ServiceId, request.TargetIds)
	if err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	tempDir, zipFilePath, err := deployService.PrepareDeployPackage(service)
	if err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}
	defer os.RemoveAll(tempDir)

	// 部署到所有目标机器
	for _, target := range targets {
		if err := deployService.DeployToTarget(target, service, zipFilePath); err != nil {
			error_response.NewErrorResponse(c, err.Error())
			return
		}
	}

	// 保存部署历史
	if err := deployService.SaveDeployHistory(service.Id, request.Comment); err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"message": "部署成功",
	})
}

