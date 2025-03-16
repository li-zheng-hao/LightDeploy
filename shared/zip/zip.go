package zip

import (
	"archive/zip"
	"compress/flate"
	"io"
	"mime/multipart"
	"os"
	"path/filepath"
)

// CompressFolderWithLevel 使用指定压缩级别压缩文件夹
func CompressFolderWithLevel(sourceDir string, targetFile string, level int) error {
	zipFile, err := os.Create(targetFile)
	if err != nil {
		return err
	}
	defer zipFile.Close()

	zipWriter := zip.NewWriter(zipFile)
	defer zipWriter.Close()

	return filepath.Walk(sourceDir, func(path string, info os.FileInfo, err error) error {
		if err != nil {
			return err
		}

		// 如果是目录，跳过文件操作
		if info.IsDir() {
			return nil
		}

		relPath, err := filepath.Rel(sourceDir, path)
		if err != nil {
			return err
		}

		// 创建新的 FileHeader 并设置时间戳
		header, err := zip.FileInfoHeader(info)
		if err != nil {
			return err
		}
		header.Name = relPath
		header.Method = zip.Deflate

		writer, err := zipWriter.CreateHeader(header)
		if err != nil {
			return err
		}

		compressor, err := flate.NewWriter(writer, level)
		if err != nil {
			return err
		}
		defer compressor.Close()

		file, err := os.Open(path)
		if err != nil {
			return err
		}
		defer file.Close()

		_, err = io.Copy(compressor, file)
		return err
	})
}

// UncompressFolder 解压zip文件到指定目录
func UncompressZipFileHeader(zipFile *multipart.FileHeader, targetDir string) error {
	file, err := zipFile.Open() // 使用 zipFile.Open() 打开文件
	if err != nil {
		return err
	}
	defer file.Close()

	reader, err := zip.NewReader(file, zipFile.Size) // 使用 zip.NewReader 创建读取器
	if err != nil {
		return err
	}

	// 确保目标目录存在
	if err := os.MkdirAll(targetDir, os.ModePerm); err != nil {
		return err
	}

	// 遍历压缩文件中的所有文件和目录
	for _, file := range reader.File {
		// 构建目标路径
		path := filepath.Join(targetDir, file.Name)

		// 如果是目录，创建它
		if file.FileInfo().IsDir() {
			if err := os.MkdirAll(path, file.Mode()); err != nil {
				return err
			}
			continue
		}

		// 确保父目录存在
		if err := os.MkdirAll(filepath.Dir(path), os.ModePerm); err != nil {
			return err
		}

		// 创建目标文件
		dstFile, err := os.OpenFile(path, os.O_WRONLY|os.O_CREATE|os.O_TRUNC, file.Mode())
		if err != nil {
			return err
		}

		// 打开压缩文件
		srcFile, err := file.Open()
		if err != nil {
			dstFile.Close()
			return err
		}

		// 复制内容
		_, err = io.Copy(dstFile, srcFile)
		
		// 关闭文件
		srcFile.Close()
		dstFile.Close()

		if err != nil {
			return err
		}

		// 设置解压后文件的修改时间
		modTime := file.Modified
		if err := os.Chtimes(path, modTime, modTime); err != nil {
			return err
		}
	}

	return nil
}


func UncompressFolder(zipFile string, targetDir string) error {
	
	return nil
}
