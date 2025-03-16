package deploy

import (
	"fmt"
	"ld_agent/internal/windows_service"
	"ld_shared/error_response"
	"ld_shared/sse"
	"ld_shared/zip"
	"mime/multipart"
	"net/http"
	"strings"

	"github.com/gin-gonic/gin"
)

type DeployWindowsServiceRequest struct {
	ServiceName string                `form:"serviceName"`
	ServicePath string                `form:"servicePath"`
	ZipFile     *multipart.FileHeader `form:"file"`  // 修改为multipart.FileHeader类型
	OnlyCopyFile bool                  `form:"onlyCopyFile"`
}

func DeployWindowsService(c *gin.Context) {
	var request DeployWindowsServiceRequest
	if err := c.ShouldBind(&request); err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}
	if request.ZipFile == nil {
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
		err := windows_service.StopService(request.ServiceName)
		if err != nil {
			sse.SendMessage("停止服务失败: " + err.Error())
			error_response.NewErrorResponse(c, err.Error())
			return
		}
	}

	// 添加解压文件的逻辑，带有重试机制
	maxRetries := 3
	var unzipErr error
	for i := 0; i < maxRetries; i++ {
		unzipErr = zip.UncompressZipFileHeader(request.ZipFile, request.ServicePath)
		if unzipErr == nil {
			sse.SendMessage("文件解压成功")
			break
		}
		sse.SendMessage(fmt.Sprintf("解压失败，第%d次重试: %s", i+1, unzipErr.Error()))
	}

	if unzipErr != nil {
		sse.SendMessage("解压文件最终失败")
		error_response.NewErrorResponse(c, unzipErr.Error())
		return
	}
	if !request.OnlyCopyFile {
		err := windows_service.StartService(request.ServiceName)
		if err != nil {
			sse.SendMessage("启动服务失败: " + err.Error())
			error_response.NewErrorResponse(c, err.Error())
			return
		}
	}
	
	c.JSON(http.StatusOK, nil)
}

