package deploy

import (
	"fmt"
	"ld_agent/internal/windows_service"
	"ld_shared/dto"
	"ld_shared/error_response"
	"ld_shared/sse"
	"ld_shared/zip"
	"math"
	"mime/multipart"
	"net/http"
	"os"
	"path/filepath"
	"strings"
	"time"

	"log/slog"

	"github.com/gin-gonic/gin"
)

type DeployWindowsServiceRequest struct {
	ServiceName  string                `form:"serviceName"`
	ServicePath  string                `form:"servicePath"`
	ZipFile      *multipart.FileHeader `form:"file"` // 修改为multipart.FileHeader类型
	OnlyCopyFile bool                  `form:"onlyCopyFile"`
}

func DeployWindowsService(c *gin.Context) {
	slog.Info("开始部署 Windows 服务")
	var request DeployWindowsServiceRequest
	if err := c.ShouldBind(&request); err != nil {
		slog.Error("请求参数绑定失败", "error", err)
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	slog.Info("验证请求参数",
		"serviceName", request.ServiceName,
		"servicePath", request.ServicePath,
		"fileName", request.ZipFile.Filename,
		"onlyCopyFile", request.OnlyCopyFile)

	if request.ZipFile == nil {
		slog.Error("未上传zip文件")
		error_response.NewErrorResponse(c, "请上传zip文件")
		return
	}
	if request.ServiceName == "" {
		error_response.NewErrorResponse(c, "请输入服务名称")
		return
	}
	if request.ServicePath == "" {
		error_response.NewErrorResponse(c, "请输入服务路径")
		return
	}
	if !strings.HasSuffix(request.ZipFile.Filename, ".zip") {
		error_response.NewErrorResponse(c, "请上传zip文件")
		return
	}
	if !request.OnlyCopyFile {
		slog.Info("准备停止服务", "serviceName", request.ServiceName)
		err := windows_service.StopService(request.ServiceName)
		if err != nil {
			slog.Error("停止服务失败", "error", err)
			sse.SendMessage("停止服务失败: " + err.Error())
			error_response.NewErrorResponse(c, err.Error())
			return
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
		error_response.NewErrorResponse(c, unzipErr.Error())
		return
	}
	if !request.OnlyCopyFile {
		slog.Info("准备启动服务", "serviceName", request.ServiceName)
		err := windows_service.StartService(request.ServiceName)
		if err != nil {
			slog.Error("启动服务失败", "error", err)
			sse.SendMessage("启动服务失败: " + err.Error())
			error_response.NewErrorResponse(c, err.Error())
			return
		}
		slog.Info("服务已启动", "serviceName", request.ServiceName)
	}

	slog.Info("部署完成")
	c.JSON(http.StatusOK, nil)
}

func CompareFiles(c *gin.Context) {
	slog.Info("开始比较文件")
	var request dto.CompareFilesRequest
	if err := c.ShouldBind(&request); err != nil {
		slog.Error("请求参数绑定失败", "error", err)
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	slog.Info("开始文件对比",
		"servicePath", request.ServicePath,
		"fileCount", len(request.FileInfos))

	if request.ServicePath == "" {
		error_response.NewErrorResponse(c, "服务路径不能为空")
		return
	}
	if len(request.FileInfos) == 0 {
		error_response.NewErrorResponse(c, "文件信息不能为空")
		return
	}
	var differentFiles []dto.CompareFileInfo
	for _, fileInfo := range request.FileInfos {
		filePath := filepath.Join(request.ServicePath, fileInfo.FileRelativePath)
		localFileInfo, err := os.Stat(filePath)
		if err != nil {
			// 本地文件不存在，认为是不同的
			differentFiles = append(differentFiles, fileInfo)
			continue
		}

		// 对比文件大小
		if localFileInfo.Size() != fileInfo.FileSize {
			differentFiles = append(differentFiles, fileInfo)
			continue
		}

		// 对比修改时间，如果差距大于5秒则认为不同
		localModTime := localFileInfo.ModTime().Unix()
		if math.Abs(float64(localModTime-fileInfo.ModifyTimeStamp)) > 5 {
			differentFiles = append(differentFiles, fileInfo)
			continue
		}
	}

	c.JSON(http.StatusOK, dto.CompareFilesResponse{
		FileInfos: differentFiles,
	})
}

func StartService(c *gin.Context) {
	slog.Info("开始启动服务")
	serviceName := c.Query("serviceName")
	if serviceName == "" {
		error_response.NewErrorResponse(c, "服务名称不能为空")
		return
	}
	err := windows_service.StartService(serviceName)
	if err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}
	c.JSON(http.StatusOK, nil)
}

func StopService(c *gin.Context) {
	slog.Info("开始停止服务")
	serviceName := c.Query("serviceName")
	if serviceName == "" {
		error_response.NewErrorResponse(c, "服务名称不能为空")
		return
	}
	err := windows_service.StopService(serviceName)
	if err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}
	c.JSON(http.StatusOK, nil)
}
