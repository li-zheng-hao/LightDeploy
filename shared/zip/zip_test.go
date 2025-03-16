package zip

import (
	"os"
	"path/filepath"
	"testing"
)

func TestZipOperations(t *testing.T) {
	t.Run("1. Compress", func(t *testing.T) {
		sourceDir, err := filepath.Abs("../../test/files/")
		if err != nil {
			t.Fatalf("获取源目录的绝对路径失败: %v", err)
		}
		t.Logf("源目录的完整路径: %s", sourceDir)
		targetFile := "../../test/target.zip"
		level := 9

		err = CompressFolderWithLevel(sourceDir, targetFile, level)
		if err != nil {
			t.Fatalf("压缩失败: %v", err)
		}

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


