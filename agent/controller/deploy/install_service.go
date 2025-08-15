package deploy

import (
	"ld_agent/internal/windows_service"
	"ld_shared/error_response"
	"ld_shared/zip"
	"log/slog"
	"mime/multipart"
	"net/http"

	"github.com/gin-gonic/gin"
)

type InstallServiceRequest struct {
	// 服务名称
	ServiceName string `form:"serviceName"`
	// 程序压缩包
	ZipFile *multipart.FileHeader `form:"file"`
	// 服务路径 文件夹路径
	ServicePath string `form:"servicePath"`
	// 程序完整执行路径
	ExePath string `form:"exePath"`
	// 程序执行参数
	ExeParams string `form:"exeParams"`
}

func InstallService(c *gin.Context) {
	slog.Info("开始安装服务")
	var request InstallServiceRequest
	if err := c.ShouldBind(&request); err != nil {
		slog.Error("请求参数绑定失败", "error", err)
		error_response.NewErrorResponse(c, err.Error())
		return
	}
	if request.ServiceName == "" {
		error_response.NewErrorResponse(c, "服务名称不能为空")
		return
	}
	if request.ZipFile == nil {
		error_response.NewErrorResponse(c, "程序压缩包不能为空")
		return
	}
	if request.ExePath == "" {
		error_response.NewErrorResponse(c, "程序路径不能为空")
		return
	}

	// 解压程序压缩包
	zip.UncompressZipFileHeader(request.ZipFile, request.ServicePath)

	// 安装服务
	windows_service.InstallService(request.ServiceName, request.ExePath, request.ExeParams)

	c.JSON(http.StatusOK, gin.H{
		"message": "服务安装成功",
	})
}
