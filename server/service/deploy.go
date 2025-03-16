package service

import (
	"bytes"
	"fmt"
	"io"
	"ld_server/db"
	"ld_server/model"
	"ld_shared/process"
	"ld_shared/sse"
	"ld_shared/zip"
	"mime/multipart"
	"net/http"
	"os"
	"path/filepath"
	"time"

	"github.com/google/uuid"
)

// DeployService 部署服务接口
type DeployService interface {
	ValidateDeployRequest(serviceId int, targetIds []int) (*model.DeployService, []model.DeployTarget, error)
	PrepareDeployPackage(service *model.DeployService) (string, string, error)
	DeployToTarget(target model.DeployTarget, service *model.DeployService, zipFilePath string) error
	SaveDeployHistory(serviceId int, comment string) error
}

type deployService struct{}

// NewDeployService 创建部署服务实例
func NewDeployService() DeployService {
	return &deployService{}
}

// ValidateDeployRequest 验证部署请求并获取服务和目标信息
func (s *deployService) ValidateDeployRequest(serviceId int, targetIds []int) (*model.DeployService, []model.DeployTarget, error) {
	var service model.DeployService
	has, err := db.Engine.ID(serviceId).Get(&service)
	if err != nil {
		return nil, nil, fmt.Errorf("查询服务失败: %v", err)
	}
	if !has {
		return nil, nil, fmt.Errorf("服务不存在")
	}

	var targets []model.DeployTarget
	err = db.Engine.In("id", targetIds).Find(&targets)
	if err != nil {
		return nil, nil, fmt.Errorf("查询目标失败: %v", err)
	}
	if len(targets) == 0 {
		return nil, nil, fmt.Errorf("目标不存在")
	}

	for _, target := range targets {
		if target.ServiceId != service.Id {
			return nil, nil, fmt.Errorf("目标服务不匹配")
		}
	}

	return &service, targets, nil
}

// PrepareDeployPackage 准备部署包
func (s *deployService) PrepareDeployPackage(service *model.DeployService) (string, string, error) {
	executablePath, err := os.Executable()
	if err != nil {
		return "", "", fmt.Errorf("获取当前程序路径失败: %v", err)
	}

	tempDir := filepath.Join(filepath.Dir(executablePath), "ld_deploy_temp", uuid.New().String())
	if err := os.MkdirAll(tempDir, 0755); err != nil {
		return "", "", fmt.Errorf("创建临时目录失败: %v", err)
	}

	zipFilePath := filepath.Join(tempDir, "deploy.zip")

	if service.ProjectType == model.ProjectTypeNetCore {
		sse.SendMessage("开始构建项目")
		process.ExecuteWindowsCommand("dotnet publish " + service.ProjectPath + " -o " + tempDir)
		sse.SendMessage("构建完成")
		sse.SendMessage("开始压缩文件")
		zip.CompressFolderWithLevel(tempDir, zipFilePath, 9)
	} else {
		sse.SendMessage("开始压缩文件")
		zip.CompressFolderWithLevel(service.ProjectPath, zipFilePath, 9)
	}
	sse.SendMessage("压缩完成")

	if fileInfo, err := os.Stat(zipFilePath); err == nil {
		sizeMB := float64(fileInfo.Size()) / 1024 / 1024
		sse.SendMessage(fmt.Sprintf("压缩包大小: %.4f MB", sizeMB))
	}

	return tempDir, zipFilePath, nil
}

// DeployToTarget 部署到目标机器
func (s *deployService) DeployToTarget(target model.DeployTarget, service *model.DeployService, zipFilePath string) error {
	sse.SendMessage("开始部署到:" + target.Host)

	file, err := os.Open(zipFilePath)
	if err != nil {
		return fmt.Errorf("打开压缩文件失败: %v", err)
	}
	defer file.Close()

	req, err := http.NewRequest("POST", fmt.Sprintf("http://%s:%d/api/deploy/deploy-windows-service", target.Host, target.Port), nil)
	if err != nil {
		return fmt.Errorf("创建HTTP请求失败: %v", err)
	}

	body := &bytes.Buffer{}
	writer := multipart.NewWriter(body)
	part, err := writer.CreateFormFile("file", "deploy.zip")
	if err != nil {
		return fmt.Errorf("创建表单失败: %v", err)
	}

	_, err = io.Copy(part, file)
	if err != nil {
		return fmt.Errorf("复制文件失败: %v", err)
	}

	writer.WriteField("serviceName", service.ServiceName)
	writer.WriteField("servicePath", target.ServicePath)
	writer.WriteField("onlyCopyFile", fmt.Sprintf("%v", service.OnlyCopyFile))
	writer.Close()

	req.Body = io.NopCloser(body)
	req.Header.Set("Content-Type", writer.FormDataContentType())

	client := &http.Client{}
	resp, err := client.Do(req)
	if err != nil {
		return fmt.Errorf("发送部署请求失败: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		bodyBytes, _ := io.ReadAll(resp.Body)
		return fmt.Errorf("部署失败，状态码：%d，错误：%s", resp.StatusCode, string(bodyBytes))
	}

	sse.SendMessage("部署完成:" + target.Host)
	return nil
}

// SaveDeployHistory 保存部署历史
func (s *deployService) SaveDeployHistory(serviceId int, comment string) error {
	history := &model.DeployHistory{
		ServiceId:  serviceId,
		DeployTime: time.Now(),
		Comment:    comment,
	}

	_, err := db.Engine.Insert(history)
	if err != nil {
		return fmt.Errorf("保存部署历史失败: %v", err)
	}
	return nil
}