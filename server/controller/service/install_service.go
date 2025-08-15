package service

import (
	"bytes"
	"fmt"
	"io"
	"mime/multipart"
	"net/http"
	"os"
	"path/filepath"
	"time"

	"ld_server/service/deploy"
	"ld_shared/error_response"
	"ld_shared/sse"

	"github.com/gin-gonic/gin"
)

type InstallServiceRequest struct {
	ServiceId int   `json:"serviceId"`
	TargetIds []int `json:"targetIds"`
}

func InstallService(c *gin.Context) {
	var request InstallServiceRequest
	if err := c.ShouldBindJSON(&request); err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	// 验证服务和目标机器
	deployService, targets, err := deploy.ValidateDeployRequest(request.ServiceId, request.TargetIds)
	if err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	// 准备部署包
	tempDir, zipFilePath, err := deploy.PrepareDeployPackage(deployService)
	if err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}
	defer os.RemoveAll(tempDir)

	// 在所有目标机器上安装服务
	for _, target := range *targets {
		sse.SendMessage(fmt.Sprintf("正在机器 %s 上安装服务", target.Host))

		// 构建multipart请求
		file, err := os.Open(zipFilePath)
		if err != nil {
			error_response.NewErrorResponse(c, fmt.Sprintf("打开文件失败: %v", err))
			return
		}
		defer file.Close()

		// 创建请求
		url := fmt.Sprintf("http://%s:%d/api/service/install-service", target.Host, target.Port)
		body := &bytes.Buffer{}
		writer := multipart.NewWriter(body)

		// 添加文件
		part, err := writer.CreateFormFile("file", filepath.Base(zipFilePath))
		if err != nil {
			error_response.NewErrorResponse(c, fmt.Sprintf("创建form文件失败: %v", err))
			return
		}
		if _, err := io.Copy(part, file); err != nil {
			error_response.NewErrorResponse(c, fmt.Sprintf("复制文件失败: %v", err))
			return
		}

		// 添加其他字段
		writer.WriteField("serviceName", deployService.ServiceName)
		writer.WriteField("exePath", target.ExePath)
		writer.WriteField("servicePath", target.ServicePath)
		writer.WriteField("exeParams", target.ExeParams)
		writer.Close()

		// 发送请求
		req, err := http.NewRequest("POST", url, body)
		if err != nil {
			error_response.NewErrorResponse(c, fmt.Sprintf("创建请求失败: %v", err))
			return
		}
		req.Header.Set("Content-Type", writer.FormDataContentType())

		client := &http.Client{
			Timeout: 5 * time.Minute,
		}
		resp, err := client.Do(req)
		if err != nil {
			error_response.NewErrorResponse(c, fmt.Sprintf("请求失败: %v", err))
			return
		}
		defer resp.Body.Close()

		if resp.StatusCode != http.StatusOK {
			bodyBytes, _ := io.ReadAll(resp.Body)
			error_response.NewErrorResponse(c, fmt.Sprintf("安装服务失败: %s", string(bodyBytes)))
			return
		}
	}

	c.JSON(http.StatusOK, gin.H{
		"message": "服务安装成功",
	})
}
