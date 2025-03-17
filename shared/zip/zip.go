package zip

import (
	"archive/zip"
	"compress/flate"
	"io"
	"log/slog"
	"mime/multipart"
	"os"
	"path/filepath"
	"strings"
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

	// 设置压缩级别
	zipWriter.RegisterCompressor(zip.Deflate, func(out io.Writer) (io.WriteCloser, error) {
		return flate.NewWriter(out, level)
	})

	return filepath.Walk(sourceDir, func(path string, info os.FileInfo, err error) error {
		if err != nil {
			return err
		}

		// 获取相对路径
		relPath, err := filepath.Rel(sourceDir, path)
		if err != nil {
			return err
		}

		// 统一使用斜杠作为路径分隔符
		relPath = strings.ReplaceAll(relPath, "\\", "/")

		// 如果是目录，创建目录条目
		if info.IsDir() {
			if relPath == "." {
				return nil
			}
			header, err := zip.FileInfoHeader(info)
			if err != nil {
				return err
			}
			header.Name = relPath + "/"
			header.Method = zip.Deflate
			header.Modified = info.ModTime()
			_, err = zipWriter.CreateHeader(header)
			return err
		}

		// 创建新的 FileHeader
		header, err := zip.FileInfoHeader(info)
		if err != nil {
			return err
		}
		header.Name = relPath
		header.Method = zip.Deflate
		header.Modified = info.ModTime()

		writer, err := zipWriter.CreateHeader(header)
		if err != nil {
			return err
		}

		file, err := os.Open(path)
		if err != nil {
			return err
		}
		defer file.Close()

		_, err = io.Copy(writer, file)
		return err
	})
}

// UncompressZipReader 从io.ReaderAt解压zip文件到指定目录
func UncompressZipReader(reader io.ReaderAt, size int64, targetDir string) error {
	zipReader, err := zip.NewReader(reader, size)
	if err != nil {
		return err
	}

	// 确保目标目录存在
	if err := os.MkdirAll(targetDir, os.ModePerm); err != nil {
		return err
	}

	// 遍历压缩文件中的所有文件和目录
	for _, file := range zipReader.File {
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
		slog.Info("解压文件", "path", path, "modTime", modTime)
		if err := os.Chtimes(path, modTime, modTime); err != nil {
			return err
		}
	}

	return nil
}

// UncompressZipFileHeader 解压zip文件到指定目录
func UncompressZipFileHeader(zipFile *multipart.FileHeader, targetDir string) error {
	file, err := zipFile.Open()
	if err != nil {
		return err
	}
	defer file.Close()

	return UncompressZipReader(file, zipFile.Size, targetDir)
}

func UncompressFolder(zipFile string, targetDir string) error {
	// 打开zip文件
	reader, err := zip.OpenReader(zipFile)
	if err != nil {
		return err
	}
	defer reader.Close()

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
