//go:build linux

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
	return "agent_new"
}

// startUpdateProcess 生成并执行 Linux 更新 shell 脚本
func startUpdateProcess(serviceName, exePath, newExePath, tempDir string) error {
	scriptContent := fmt.Sprintf(`#!/bin/bash
sleep 2
systemctl stop %s
cp -f "%s" "%s"
chmod +x "%s"
systemctl start %s
rm -f "%s"
rm -f "$0"
`, serviceName, newExePath, exePath, exePath, serviceName, newExePath)

	scriptPath := filepath.Join(tempDir, "update.sh")
	if err := os.WriteFile(scriptPath, []byte(scriptContent), 0755); err != nil {
		return fmt.Errorf("创建更新脚本失败: %v", err)
	}

	cmd := exec.Command("/bin/bash", scriptPath)
	// 脱离当前进程组，避免 agent 退出时脚本被一并终止
	cmd.SysProcAttr = &syscall.SysProcAttr{
		Setsid: true,
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