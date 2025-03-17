package deploy

import (
	"bufio"
	"bytes"
	"encoding/json"
	"fmt"
	"io"
	"ld_server/db"
	"ld_server/model"
	"ld_shared/dto"
	"ld_shared/process"
	"ld_shared/sse"
	"ld_shared/zip"
	"mime/multipart"
	"net/http"
	"os"
	"path/filepath"
	"strings"
	"time"

	"github.com/google/uuid"
)

const (
	operationTimeout = 30 * time.Second
)

// StartServiceOnTarget 在目标机器上启动服务
func StartServiceOnTarget(target *model.DeployTarget, service *model.DeployService) error {
	url := fmt.Sprintf("http://%s:%d/api/deploy/start-service?serviceName=%s",
		target.Host,
		target.Port,
		service.ServiceName,
	)

	// 使用重试机制调用agent接口
	var err error
	client := &http.Client{
		Timeout: operationTimeout,
	}

	req, err := http.NewRequest(http.MethodPost, url, nil)
	if err != nil {
		return fmt.Errorf("创建HTTP请求失败: %v", err)
	}

	resp, err := client.Do(req)
	if err == nil && resp.StatusCode == http.StatusOK {
		resp.Body.Close()
		return nil
	}
	if resp != nil {
		resp.Body.Close()
	}

	sse.SendMessage(fmt.Sprintf("启动服务失败: %v", err))
	return fmt.Errorf("启动服务失败: %v", err)
}

// StopServiceOnTarget 在目标机器上停止服务
func StopServiceOnTarget(target *model.DeployTarget, service *model.DeployService) error {
	url := fmt.Sprintf("http://%s:%d/api/deploy/stop-service?serviceName=%s",
		target.Host,
		target.Port,
		service.ServiceName,
	)

	// 使用重试机制调用agent接口
	var err error
	client := &http.Client{
		Timeout: operationTimeout,
	}

	req, err := http.NewRequest(http.MethodPost, url, nil)
	if err != nil {
		return fmt.Errorf("创建HTTP请求失败: %v", err)
	}

	resp, err := client.Do(req)
	if err == nil && resp.StatusCode == http.StatusOK {
		resp.Body.Close()
		return nil
	}
	if resp != nil {
		resp.Body.Close()
	}

	sse.SendMessage(fmt.Sprintf("停止服务失败: %v", err))
	return fmt.Errorf("停止服务失败: %v", err)
}

// ValidateDeployRequest 验证部署请求并获取服务和目标信息
func ValidateDeployRequest(serviceId int, targetIds []int) (*model.DeployService, *[]model.DeployTarget, error) {
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

	return &service, &targets, nil
}

// PrepareDeployPackage 准备部署包
func PrepareDeployPackage(service *model.DeployService) (string, string, error) {
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
		zip.CompressFolderWithLevel(tempDir, zipFilePath, 3)
	} else {
		sse.SendMessage("开始压缩文件")
		zip.CompressFolderWithLevel(service.ProjectPath, zipFilePath, 3)
	}
	sse.SendMessage("压缩完成")

	if fileInfo, err := os.Stat(zipFilePath); err == nil {
		sizeMB := float64(fileInfo.Size()) / 1024 / 1024
		sse.SendMessage(fmt.Sprintf("压缩包大小: %.4f MB", sizeMB))
	}

	return tempDir, zipFilePath, nil
}

// agentSSEConnection 用于管理agent SSE连接
type agentSSEConnection struct {
	done chan struct{}
}

// connectToAgentSSE 连接到agent的SSE接口
func connectToAgentSSE(host string, port int) (*agentSSEConnection, error) {
	conn := &agentSSEConnection{
		done: make(chan struct{}),
	}

	client := &http.Client{
		Timeout: time.Minute * 5,
	}
	resp, err := client.Get(fmt.Sprintf("http://%s:%d/api/sse/sse", host, port))
	if err != nil {
		return nil, fmt.Errorf("连接agent SSE失败: %v", err)
	}

	go func() {
		defer resp.Body.Close()
		reader := bufio.NewReader(resp.Body)
		for {
			select {
			case <-conn.done:
				return
			default:
				line, err := reader.ReadString('\n')
				if err != nil {
					if err != io.EOF {
						sse.SendMessage(fmt.Sprintf("agent SSE连接断开: %v", err))
					}
					return
				}
				if strings.HasPrefix(line, "data: ") {
					message := strings.TrimPrefix(line, "data: ")
					message = strings.TrimSpace(message)
					sse.SendMessage("Agent消息:" + message)
				}
			}
		}
	}()

	return conn, nil
}

// closeAgentSSE 关闭agent SSE连接
func closeAgentSSE(conn *agentSSEConnection) {
	if conn != nil {
		close(conn.done)
	}
}

// DeployToTarget 部署到目标机器
func DeployToTarget(target model.DeployTarget, service *model.DeployService, zipFilePath string) error {
	sse.SendMessage("开始部署到:" + target.Host)

	// 连接agent的SSE
	conn, err := connectToAgentSSE(target.Host, target.Port)
	if err != nil {
		sse.SendMessage(fmt.Sprintf("警告：无法连接到agent的SSE接口: %v", err))
	}
	// 确保在函数返回时关闭SSE连接
	defer closeAgentSSE(conn)

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
func SaveDeployHistory(serviceId int, comment string) error {
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

// CollectSourceFileInfos 收集源目录下所有文件的信息
func CollectSourceFileInfos(sourceDir string) ([]dto.CompareFileInfo, error) {
	var fileInfos []dto.CompareFileInfo
	err := filepath.Walk(sourceDir, func(path string, info os.FileInfo, err error) error {
		if err != nil {
			return err
		}
		if !info.IsDir() {
			relPath, err := filepath.Rel(sourceDir, path)
			if err != nil {
				return err
			}
			fileInfos = append(fileInfos, dto.CompareFileInfo{
				FileRelativePath: relPath,
				FileSize:         info.Size(),
				ModifyTimeStamp:  info.ModTime().Unix(),
			})
		}
		return nil
	})
	return fileInfos, err
}

// CompareFilesWithTarget 与目标机器比较文件差异
func CompareFilesWithTarget(target model.DeployTarget, fileInfos []dto.CompareFileInfo) (*dto.CompareFilesResponse, error) {
	compareRequest := dto.CompareFilesRequest{
		ServicePath: target.ServicePath,
		FileInfos:   fileInfos,
	}

	jsonData, err := json.Marshal(compareRequest)
	if err != nil {
		return nil, err
	}

	client := &http.Client{}
	resp, err := client.Post(
		fmt.Sprintf("http://%s:%d/api/deploy/compare-files", target.Host, target.Port),
		"application/json",
		bytes.NewReader(jsonData))
	if err != nil {
		return nil, err
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return nil, fmt.Errorf("compare files failed with status: %d", resp.StatusCode)
	}

	var compareResult dto.CompareFilesResponse
	if err := json.NewDecoder(resp.Body).Decode(&compareResult); err != nil {
		return nil, err
	}

	return &compareResult, nil
}

// PrepareUpdateFiles 准备需要更新的文件
func PrepareUpdateFiles(sourceDir string, compareResult *dto.CompareFilesResponse) (string, error) {
	if len(compareResult.FileInfos) == 0 {
		return "", nil
	}

	// 创建新的临时目录
	newTempDir := filepath.Join(filepath.Dir(sourceDir), fmt.Sprintf("filtered_%s", uuid.New().String()))
	if err := os.MkdirAll(newTempDir, 0755); err != nil {
		return "", err
	}

	// 复制需要更新的文件
	for _, file := range compareResult.FileInfos {
		srcPath := filepath.Join(sourceDir, file.FileRelativePath)
		dstPath := filepath.Join(newTempDir, file.FileRelativePath)

		if err := os.MkdirAll(filepath.Dir(dstPath), 0755); err != nil {
			return "", err
		}

		if err := copyFile(srcPath, dstPath); err != nil {
			return "", err
		}
	}

	return newTempDir, nil
}

// FastDeployToTarget 快速模式部署到目标机器
func FastDeployToTarget(target *model.DeployTarget, deployService *model.DeployService, sourceDir string) error {
	// 连接agent的SSE
	conn, err := connectToAgentSSE(target.Host, target.Port)
	if err != nil {
		sse.SendMessage(fmt.Sprintf("警告：无法连接到agent的SSE接口: %v", err))
	}
	// 确保在函数返回时关闭SSE连接
	defer closeAgentSSE(conn)

	// 收集文件信息
	fileInfos, err := CollectSourceFileInfos(sourceDir)
	if err != nil {
		return fmt.Errorf("收集文件信息失败: %v", err)
	}

	// 比较文件
	compareResult, err := CompareFilesWithTarget(*target, fileInfos)
	if err != nil {
		return fmt.Errorf("比较文件失败: %v", err)
	}

	if len(compareResult.FileInfos) == 0 {
		sse.SendMessage(fmt.Sprintf("目标机器 %s 没有需要更新的文件", target.Host))
		return nil
	}

	// 准备更新文件
	filteredDir, err := PrepareUpdateFiles(sourceDir, compareResult)
	if err != nil {
		return fmt.Errorf("准备更新文件失败: %v", err)
	}
	if filteredDir == "" {
		return nil
	}
	defer os.RemoveAll(filteredDir)

	// 打包文件
	zipFilePath := filepath.Join(filepath.Dir(sourceDir), fmt.Sprintf("deploy_%s.zip", uuid.New().String()))
	if err := zip.CompressFolderWithLevel(filteredDir, zipFilePath, 3); err != nil {
		return fmt.Errorf("压缩文件失败: %v", err)
	}
	defer os.Remove(zipFilePath)

	sse.SendMessage(fmt.Sprintf("目标机器 %s 需要更新 %d 个文件", target.Host, len(compareResult.FileInfos)))

	// 部署到目标机器
	if err := DeployToTarget(*target, deployService, zipFilePath); err != nil {
		return fmt.Errorf("部署到目标机器 %s 失败: %v", target.Host, err)
	}

	return nil
}

func copyFile(src, dst string) error {
	input, err := os.ReadFile(src)
	if err != nil {
		return err
	}
	return os.WriteFile(dst, input, 0644)
}
