package deploy

import (
	"os"
	"path/filepath"
	"regexp"
	"testing"
)

func TestRegexMatching(t *testing.T) {
	// 测试的正则表达式
	ignoreFileRegex := `^(\.cursor|\.venv|\.output|__pycache__)[\\/].*`

	// 测试路径
	testPath := "E:\\test"

	t.Log("\n=== 测试真正的正则表达式 ===")
	// 编译正则表达式
	re, err := regexp.Compile(ignoreFileRegex)
	if err != nil {
		t.Errorf("正则表达式编译错误: %v", err)
		return
	}

	filepath.Walk(testPath, func(path string, info os.FileInfo, err error) error {
		if err != nil {
			return err
		}
		if !info.IsDir() {
			relPath, err := filepath.Rel(testPath, path)
			if err != nil {
				return err
			}
			matched := re.MatchString(relPath)
			t.Logf("文件路径: %s, 是否匹配: %v", relPath, matched)
		}
		return nil
	})

}
