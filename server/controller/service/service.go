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

	"ld_server/db"
	"ld_server/model"
	"ld_server/service/deploy"
	"ld_server/service/target"
	"ld_shared/error_response"
	"ld_shared/sse"

	"github.com/gin-gonic/gin"
)

func GetServiceStatus(c *gin.Context) {
	serviceId := c.Param("serviceId")
	if serviceId == "" {
		error_response.NewErrorResponse(c, "serviceId is required")
		return
	}

	serviceModel := new(model.DeployService)
	_, err := db.Engine.ID(serviceId).Get(serviceModel)
	if err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	// 获取服务关联的目标机器
	var targets []model.DeployTarget
	err = db.Engine.Where("service_id = ?", serviceId).Find(&targets)
	if err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	if len(targets) == 0 {
		error_response.NewErrorResponse(c, "no targets found for this service")
		return
	}

	results, err := target.GetServiceStatus(int(serviceModel.Id))
	if err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	c.JSON(http.StatusOK, results)
}

// 创建部署服务
func CreateDeployService(c *gin.Context) {
	var service model.DeployService
	if err := c.ShouldBindJSON(&service); err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	// 强制设置id为0，确保使用数据库自增id
	service.Id = 0

	// 调用数据库创建
	affected, err := db.Engine.Insert(&service)
	if err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"affected": affected,
		"data":     service,
	})
}

// 更新部署服务
func UpdateDeployService(c *gin.Context) {
	var service model.DeployService
	if err := c.ShouldBindJSON(&service); err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	if service.Id <= 0 {
		error_response.NewErrorResponse(c, "id is required")
		return
	}

	// 调用数据库更新
	affected, err := db.Engine.ID(service.Id).UseBool("only_copy_file").Update(&service)
	if err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"affected": affected,
	})
}

// 获取所有部署服务
func GetDeployServices(c *gin.Context) {
	var services []model.DeployService
	err := db.Engine.Find(&services)
	if err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	c.JSON(http.StatusOK, services)
}

// 删除部署服务
func DeleteDeployService(c *gin.Context) {
	id := c.Param("id")
	if id == "" {
		error_response.NewErrorResponse(c, "id is required")
		return
	}

	service := new(model.DeployService)
	affected, err := db.Engine.ID(id).Delete(service)
	if err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"affected": affected,
	})
}

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
