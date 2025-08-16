package deploy

import (
	"ld_agent/internal/windows_service"
	"ld_shared/error_response"
	"ld_shared/zip"
	"log/slog"
	"mime/multipart"

	"github.com/gofiber/fiber/v2"
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

func InstallService(c *fiber.Ctx) error {
	slog.Info("开始安装服务")
	var request InstallServiceRequest
	if err := c.BodyParser(&request); err != nil {
		slog.Error("请求参数绑定失败", "error", err)
		return error_response.NewErrorResponse(c, err.Error())
	}

	file, err := c.FormFile("file")
	if err != nil {
		return error_response.NewErrorResponse(c, "程序压缩包不能为空")
	}
	request.ZipFile = file

	if request.ServiceName == "" {
		return error_response.NewErrorResponse(c, "服务名称不能为空")
	}
	if request.ExePath == "" {
		return error_response.NewErrorResponse(c, "程序路径不能为空")
	}

	// 解压程序压缩包
	zip.UncompressZipFileHeader(request.ZipFile, request.ServicePath)

	// 安装服务
	windows_service.InstallService(request.ServiceName, request.ExePath, request.ExeParams)

	return c.JSON(fiber.Map{
		"message": "服务安装成功",
	})
}