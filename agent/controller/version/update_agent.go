package version

import (
	"log/slog"
	"os"
	"path/filepath"
	"time"

	"ld_shared/error_response"

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
	if err := os.MkdirAll(tempDir, 0755); err != nil {
		return error_response.NewErrorResponse(c, "创建临时目录失败: "+err.Error())
	}

	// 保存新文件到临时目录
	newExePath := filepath.Join(tempDir, newAgentFileName())
	if err := c.SaveFile(file, newExePath); err != nil {
		return error_response.NewErrorResponse(c, "保存更新文件失败: "+err.Error())
	}

	// 启动平台相关的更新流程（Windows 批处理 / Linux shell 脚本）
	if err := startUpdateProcess(request.AgentServiceName, exePath, newExePath, tempDir); err != nil {
		slog.Error("启动更新脚本失败", "error", err)
		return error_response.NewErrorResponse(c, err.Error())
	}

	go func() {
		time.Sleep(time.Second)
		os.Exit(0)
	}()

	return c.JSON(fiber.Map{
		"message": "更新程序已启动，服务将在几秒后重启",
	})
}