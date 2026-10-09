//go:build windows

package version

import (
	"fmt"
	"os"
	"os/exec"
	"path/filepath"
	"syscall"
)

// newAgentFileName 更新文件在临时目录中的名称
func newAgentFileName() string {
	return "agent_new.exe"
}

// startUpdateProcess 生成并执行 Windows 更新批处理脚本
func startUpdateProcess(serviceName, exePath, newExePath, tempDir string) error {
	batContent := fmt.Sprintf(`@echo off
net stop %s
timeout /t 2 /nobreak
copy /y "%s" "%s"
net start %s
del "%s"
del "%%~f0"
`, serviceName, newExePath, exePath, serviceName, newExePath)

	batPath := filepath.Join(tempDir, "update.bat")
	if err := os.WriteFile(batPath, []byte(batContent), 0755); err != nil {
		return fmt.Errorf("创建更新脚本失败: %v", err)
	}

	cmd := exec.Command("cmd", "/c", batPath)
	// 设置进程属性，使用新进程组
	cmd.SysProcAttr = &syscall.SysProcAttr{
		CreationFlags: syscall.CREATE_NEW_PROCESS_GROUP,
	}

	if err := cmd.Start(); err != nil {
		return fmt.Errorf("启动更新脚本失败: %v", err)
	}
	// 释放子进程，使其独立运行
	if err := cmd.Process.Release(); err != nil {
		return fmt.Errorf("释放更新进程失败: %v", err)
	}
	return nil
}