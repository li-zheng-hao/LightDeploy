package service

import (
	"ld_agent/internal/windows_service"
	"ld_shared/error_response"
	"ld_shared/zip"
	"log/slog"
	"mime/multipart"
	"net/http"

	"golang.org/x/sys/windows/svc"
	"golang.org/x/sys/windows/svc/mgr"

	"github.com/gin-gonic/gin"
)

func GetWindowsServiceStatus(c *gin.Context) {
	serviceName := c.Query("serviceName")
	if serviceName == "" {
		error_response.NewErrorResponse(c, "serviceName is required")
		return
	}

	// 连接服务管理器
	m, err := mgr.Connect()
	if err != nil {
		error_response.NewErrorResponse(c, "无法连接服务管理器: "+err.Error())
		return
	}
	defer m.Disconnect()

	// 打开指定服务
	s, err := m.OpenService(serviceName)
	if err != nil {
		error_response.NewErrorResponse(c, "服务不存在: "+err.Error())
		return
	}
	defer s.Close()

	// 查询服务状态
	status, err := s.Query()
	if err != nil {
		error_response.NewErrorResponse(c, "无法查询服务状态: "+err.Error())
		return
	}

	// 将状态码转换为可读的状态描述
	statusText := getStatusText(status.State)

	c.JSON(http.StatusOK, gin.H{
		"serviceName": serviceName,
		"status":      statusText,
		"state":       status.State,
	})
}

// 将状态码转换为可读的文本
func getStatusText(state svc.State) string {
	switch state {
	case svc.Stopped:
		return "已停止"
	case svc.StartPending:
		return "正在启动"
	case svc.StopPending:
		return "正在停止"
	case svc.Running:
		return "正在运行"
	case svc.ContinuePending:
		return "正在继续"
	case svc.PausePending:
		return "正在暂停"
	case svc.Paused:
		return "已暂停"
	default:
		return "未知状态"
	}
}

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
