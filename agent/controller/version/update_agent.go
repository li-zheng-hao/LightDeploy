package version

import (
	"fmt"
	"ld_shared/error_response"
	"log/slog"
	"os"
	"os/exec"
	"path/filepath"
	"syscall"
	"time"

	"github.com/gofiber/fiber/v2"
)

type UpdateAgentRequest struct {
	AgentServiceName string `form:"agentServiceName"`
}

func UpdateAgent(c *fiber.Ctx) error {
	var request UpdateAgentRequest
	if err := c.BodyParser(&request); err != nil {
		return error_response.NewErrorResponse(c, err.Error())
	}

	file, err := c.FormFile("file")
	if err != nil {
		return error_response.NewErrorResponse(c, "请上传更新文件")
	}

	// 获取当前程序路径
	exePath, err := os.Executable()
	if err != nil {
		return error_response.NewErrorResponse(c, "获取程序路径失败: "+err.Error())
	}

	// 创建临时目录
	tempDir := filepath.Join(filepath.Dir(exePath), "temp")
	os.MkdirAll(tempDir, 0755)

	// 保存新文件到临时目录
	newExePath := filepath.Join(tempDir, "agent_new.exe")
	if err := c.SaveFile(file, newExePath); err != nil {
		return error_response.NewErrorResponse(c, "保存更新文件失败: "+err.Error())
	}

	// 创建更新批处理文件
	batContent := fmt.Sprintf(`@echo off
net stop %s
timeout /t 2 /nobreak
copy /y "%s" "%s"
net start %s
del "%s"
del "%%~f0"
`, request.AgentServiceName, newExePath, exePath, request.AgentServiceName, newExePath)
	batPath := filepath.Join(tempDir, "update.bat")
	if err := os.WriteFile(batPath, []byte(batContent), 0755); err != nil {
		return error_response.NewErrorResponse(c, "创建更新脚本失败: "+err.Error())
	}

	// 执行更新批处理
	cmd := exec.Command("cmd", "/c", batPath)
	// 设置进程属性，使用新进程组
	cmd.SysProcAttr = &syscall.SysProcAttr{
		CreationFlags: syscall.CREATE_NEW_PROCESS_GROUP,
	}

	if err := cmd.Start(); err != nil {
		slog.Error("启动更新脚本失败", "error", err)
		return error_response.NewErrorResponse(c, "启动更新脚本失败: "+err.Error())
	}

	// 释放子进程，使其独立运行
	if err := cmd.Process.Release(); err != nil {
		slog.Error("释放更新进程失败", "error", err)
		return error_response.NewErrorResponse(c, "释放更新进程失败: "+err.Error())
	}

	go func() {
		time.Sleep(time.Second)
		os.Exit(0)
	}()

	return c.JSON(fiber.Map{
		"message": "更新程序已启动，服务将在几秒后重启",
	})
}
