package version

import (
	"fmt"
	"ld_shared/error_response"
	"mime/multipart"
	"net/http"
	"os"
	"os/exec"
	"path/filepath"
	"syscall"
	"time"

	"github.com/gin-gonic/gin"
)

func GetVersion(c *gin.Context) {
	c.JSON(http.StatusOK, gin.H{
		"version": "1.0.2",
	})
}
type UpdateAgentRequest struct {
    File *multipart.FileHeader `form:"file"`
	AgentServiceName string `form:"agentServiceName"`
}

func UpdateAgent(c *gin.Context) {
    var request UpdateAgentRequest
    if err := c.ShouldBind(&request); err != nil {
        error_response.NewErrorResponse(c, err.Error())
        return
    }

    if request.File == nil {
        error_response.NewErrorResponse(c, "请上传更新文件")
        return
    }

    // 获取当前程序路径
    exePath, err := os.Executable()
    if err != nil {
        error_response.NewErrorResponse(c, "获取程序路径失败: "+err.Error())
        return
    }

    // 创建临时目录
    tempDir := filepath.Join(filepath.Dir(exePath), "temp")
    os.MkdirAll(tempDir, 0755)

    // 保存新文件到临时目录
    newExePath := filepath.Join(tempDir, "agent_new.exe")
    if err := c.SaveUploadedFile(request.File, newExePath); err != nil {
        error_response.NewErrorResponse(c, "保存更新文件失败: "+err.Error())
        return
    }

    // 创建更新批处理文件
    batContent := fmt.Sprintf(`@echo off
net stop %s
timeout /t 2 /nobreak
copy /y "%s" "%s"
net start %s
del "%s"
del "%%~f0"
`, request.AgentServiceName,newExePath, exePath,request.AgentServiceName, newExePath)

    batPath := filepath.Join(tempDir, "update.bat")
    if err := os.WriteFile(batPath, []byte(batContent), 0755); err != nil {
        error_response.NewErrorResponse(c, "创建更新脚本失败: "+err.Error())
        return
    }

    // 执行更新批处理
    cmd := exec.Command("cmd", "/c", batPath)
    // 设置进程属性，使用新进程组
    cmd.SysProcAttr = &syscall.SysProcAttr{
        CreationFlags: syscall.CREATE_NEW_PROCESS_GROUP,
    }
    
    if err := cmd.Start(); err != nil {
        error_response.NewErrorResponse(c, "启动更新脚本失败: "+err.Error())
        return
    }
    
    // 释放子进程，使其独立运行
    if err := cmd.Process.Release(); err != nil {
        error_response.NewErrorResponse(c, "释放更新进程失败: "+err.Error())
        return
    }

    c.JSON(http.StatusOK, gin.H{
        "message": "更新程序已启动，服务将在几秒后重启",
    })

    // 延迟退出程序
    go func() {
        time.Sleep(time.Second)
        os.Exit(0)
    }()
}