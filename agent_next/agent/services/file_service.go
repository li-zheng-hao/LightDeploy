package services

import (
	"archive/zip"
	"io"
	"os"
	"path/filepath"
)

// ExtractZipFile 解压zip文件到指定目录
func ExtractZipFile(zipFile io.Reader, targetDir string) error {
	// 创建临时文件保存上传的zip
	tempFile, err := os.CreateTemp("", "upload-*.zip")
	if err != nil {
		return err
	}
	defer os.Remove(tempFile.Name())
	defer tempFile.Close()

	// 复制上传的文件到临时文件
	if _, err := io.Copy(tempFile, zipFile); err != nil {
		return err
	}

	// 打开zip文件
	reader, err := zip.OpenReader(tempFile.Name())
	if err != nil {
		return err
	}
	defer reader.Close()

	// 确保目标目录存在
	if err := os.MkdirAll(targetDir, 0755); err != nil {
		return err
	}

	// 解压所有文件
	for _, file := range reader.File {
		path := filepath.Join(targetDir, file.Name)

		if file.FileInfo().IsDir() {
			os.MkdirAll(path, 0755)
			continue
		}

		// 创建目标文件的目录
		if err := os.MkdirAll(filepath.Dir(path), 0755); err != nil {
			return err
		}

		// 创建目标文件
		dstFile, err := os.Create(path)
		if err != nil {
			return err
		}

		// 打开zip中的文件
		srcFile, err := file.Open()
		if err != nil {
			dstFile.Close()
			return err
		}

		// 复制内容
		_, err = io.Copy(dstFile, srcFile)
		srcFile.Close()
		dstFile.Close()

		if err != nil {
			return err
		}
	}

	return nil
}
