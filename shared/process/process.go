package process

import (
	"log/slog"
	"os/exec"
)

func ExecuteWindowsCommand(command string) (string, error) {
	cmd := exec.Command("cmd", "/c", command)
	slog.Info("执行Windows命令", "command", command)
	
	output, err := cmd.Output() // 这一行真正执行了命令
	if err != nil {
		slog.Error("命令执行失败", "error", err)
		return "", err
	}
	
	result := string(output)
	slog.Info("命令执行成功", "output", result)
	return result, nil
}
