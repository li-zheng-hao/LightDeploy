package env

import (
	"os"
	"strings"
)

func IsDebugMode() bool {
	// 检查可执行文件路径是否在临时目录中
	execPath, err := os.Executable()
	if err != nil {
		return false
	}

	// go run 会在临时目录中创建并执行程序
	tempDir := os.TempDir()
	return strings.HasPrefix(execPath, tempDir)
}