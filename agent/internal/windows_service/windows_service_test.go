package windows_service

import (
	"fmt"
	"testing"
)

func TestInstallService(t *testing.T) {
	// 使用反引号避免转义问题，或直接使用单引号
	exePath := fmt.Sprintf(`"%s" %s`, "C:/Users/Administrator/Desktop/test.exe", "test")
	fmt.Println(exePath)
}
