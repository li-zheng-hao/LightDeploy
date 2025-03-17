package zip

import (
	"bytes"
	"os"
	"path/filepath"
	"testing"
	"time"
)

// 自定义的File实现
type testFile struct {
	*bytes.Reader
}

func (f *testFile) Close() error {
	return nil
}

func TestZipOperations(t *testing.T) {
	t.Run("1. Compress", func(t *testing.T) {
		sourceDir, err := filepath.Abs("../../test/files")
		if err != nil {
			t.Fatalf("获取源目录的绝对路径失败: %v", err)
		}
		t.Logf("源目录的完整路径: %s", sourceDir)
		targetFile := "../../test/target.zip"
		level := 3

		// 添加开始时间记录
		startTime := time.Now()

		err = CompressFolderWithLevel(sourceDir, targetFile, level)
		if err != nil {
			t.Fatalf("压缩失败: %v", err)
		}

		// 计算并打印耗时
		elapsed := time.Since(startTime)
		t.Logf("压缩耗时: %v", elapsed)

		// 获取压缩后文件的大小
		zipInfo, err := os.Stat(targetFile)
		if err != nil {
			t.Fatalf("获取压缩文件信息失败: %v", err)
		}

		// 计算源目录总大小的函数
		var totalSize int64
		err = filepath.Walk(sourceDir, func(path string, info os.FileInfo, err error) error {
			if err != nil {
				return err
			}
			if !info.IsDir() {
				totalSize += info.Size()
			}
			return nil
		})
		if err != nil {
			t.Fatalf("计算源目录大小失败: %v", err)
		}

		// 计算压缩率
		compressionRatio := float64(zipInfo.Size()) / float64(totalSize) * 100
		t.Logf("源文件总大小: %d 字节", totalSize)
		t.Logf("压缩后大小: %d 字节", zipInfo.Size())
		t.Logf("压缩率: %.2f%%", compressionRatio)

		t.Logf("压缩完成: %s", targetFile)
		if _, err := os.Stat(targetFile); os.IsNotExist(err) {
			t.Fatalf("目标文件不存在: %s", targetFile)
		} else {
			t.Logf("目标文件存在: %s", targetFile)
		}
	})

	t.Run("2. Uncompress", func(t *testing.T) {
		zipFile := "../../test/target.zip"
		targetDir := "../../test/uncompressed"

		err := UncompressFolder(zipFile, targetDir)
		if err != nil {
			t.Fatalf("解压失败: %v", err)
		}

		t.Logf("解压完成: %s", targetDir)
	})
	t.Run("3. UncompressZipFileHeader", func(t *testing.T) {
		// 压缩测试文件
		testZipFile, err := filepath.Abs("../../test/target.zip")
		if err != nil {
			t.Fatalf("获取测试zip文件的绝对路径失败: %v", err)
		}

		// 读取zip文件内容
		zipContent, err := os.ReadFile(testZipFile)
		if err != nil {
			t.Fatalf("读取zip文件失败: %v", err)
		}

		// 设置目标目录
		targetDir := "../../test/uncompressedWithFileHeader"
		targetDir, err = filepath.Abs(targetDir)
		if err != nil {
			t.Fatalf("获取目标目录的绝对路径失败: %v", err)
		}

		// 执行解压
		err = UncompressZipReader(bytes.NewReader(zipContent), int64(len(zipContent)), targetDir)
		if err != nil {
			t.Fatalf("解压失败: %v", err)
		}

		// 验证解压结果
		if _, err := os.Stat(targetDir); os.IsNotExist(err) {
			t.Fatalf("解压目录不存在: %s", targetDir)
		}

		t.Logf("测试完成")
	})
	t.Run("3. Clean Up", func(t *testing.T) {
		targetFile := "../../test/target.zip"
		targetDir := "../../test/uncompressed"

		// 删除压缩文件
		if err := os.Remove(targetFile); err != nil {
			t.Fatalf("删除压缩文件失败: %v", err)
		}
		t.Logf("成功删除压缩文件: %s", targetFile)

		// 删除解压目录
		if err := os.RemoveAll(targetDir); err != nil {
			t.Fatalf("删除解压目录失败: %v", err)
		}
		t.Logf("成功删除解压目录: %s", targetDir)
	})
}
