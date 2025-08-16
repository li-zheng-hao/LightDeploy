package deploy

import (
	"fmt"
	"ld_agent/internal/windows_service"
	"ld_shared/error_response"
	"ld_shared/sse"
	"ld_shared/zip"
	"mime/multipart"
	"strings"
	"time"

	"log/slog"

	"github.com/gofiber/fiber/v2"
	"golang.org/x/sys/windows/svc"
)

type DeployWindowsServiceRequest struct {
	ServiceName  string                `form:"serviceName"`
	ServicePath  string                `form:"servicePath"`
	ZipFile      *multipart.FileHeader `form:"file"`
	OnlyCopyFile bool                  `form:"onlyCopyFile"`
}

func DeployWindowsService(c *fiber.Ctx) error {
	slog.Info("开始部署 Windows 服务")
	var request DeployWindowsServiceRequest
	if err := c.BodyParser(&request); err != nil {
		slog.Error("请求参数绑定失败", "error", err)
		return error_response.NewErrorResponse(c, err.Error())
	}

	file, err := c.FormFile("file")
	if err != nil {
		slog.Error("未上传zip文件")
		return error_response.NewErrorResponse(c, "请上传zip文件")
	}
	request.ZipFile = file

	slog.Info("验证请求参数",
		"serviceName", request.ServiceName,
		"servicePath", request.ServicePath,
		"fileName", request.ZipFile.Filename,
		"onlyCopyFile", request.OnlyCopyFile)

	if request.ServiceName == "" {
		return error_response.NewErrorResponse(c, "请输入服务名称")
	}
	if request.ServicePath == "" {
		return error_response.NewErrorResponse(c, "请输入服务路径")
	}
	if !strings.HasSuffix(request.ZipFile.Filename, ".zip") {
		return error_response.NewErrorResponse(c, "请上传zip文件")
	}
	var status *svc.State
	if !request.OnlyCopyFile {
		status, err = windows_service.GetServiceStatus(request.ServiceName)
		if err != nil {
			slog.Error("获取服务状态失败", "error", err)
			return error_response.NewErrorResponse(c, err.Error())
		}
	}

	if !request.OnlyCopyFile && *status == svc.Running {
		slog.Info("准备停止服务", "serviceName", request.ServiceName)
		err := windows_service.StopService(request.ServiceName)
		if err != nil {
			slog.Error("停止服务失败", "error", err)
			sse.SendMessage("停止服务失败: " + err.Error())
			return error_response.NewErrorResponse(c, err.Error())
		}
		slog.Info("服务已停止", "serviceName", request.ServiceName)
	}

	// 添加解压文件的逻辑，带有重试机制
	slog.Info("开始解压文件", "targetPath", request.ServicePath)
	maxRetries := 5
	var unzipErr error
	for i := 0; i < maxRetries; i++ {
		slog.Info("尝试解压文件", "attempt", i+1)
		unzipErr = zip.UncompressZipFileHeader(request.ZipFile, request.ServicePath)
		if unzipErr == nil {
			slog.Info("文件解压成功")
			sse.SendMessage("文件解压成功")
			break
		}
		slog.Error("解压失败，准备重试", "attempt", i+1, "error", unzipErr)
		sse.SendMessage(fmt.Sprintf("解压失败，第%d次重试: %s", i+1, unzipErr.Error()))
		time.Sleep(3 * time.Second)
	}

	if unzipErr != nil {
		slog.Error("解压文件最终失败", "error", unzipErr)
		sse.SendMessage("解压文件最终失败")
		return error_response.NewErrorResponse(c, unzipErr.Error())
	}
	if !request.OnlyCopyFile {
		maxRetries := 3
		retryInterval := 2 * time.Second
		var startErr error
		for i := 0; i < maxRetries; i++ {
			slog.Info("准备启动服务", "serviceName", request.ServiceName, "attempt", i+1)
			startErr = windows_service.StartService(request.ServiceName)
			if startErr == nil {
				slog.Info("服务已启动", "serviceName", request.ServiceName)
				break
			}
			slog.Error("启动服务失败，准备重试", "attempt", i+1, "error", startErr)
			sse.SendMessage(fmt.Sprintf("启动服务失败，第%d次重试: %s", i+1, startErr.Error()))
			time.Sleep(retryInterval)
		}

		if startErr != nil {
			slog.Error("启动服务最终失败", "error", startErr)
			sse.SendMessage("启动服务最终失败: " + startErr.Error())
			return error_response.NewErrorResponse(c, startErr.Error())
		}
	}

	slog.Info("部署完成")
	return c.JSON(nil)
}