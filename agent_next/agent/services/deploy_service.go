package services

import (
	"archive/zip"
	"fmt"
	"io"
	"net/http"
	"os"
	"path/filepath"
	"time"

	"lightdeploy.agentclient/db"
	"lightdeploy.agentclient/log"
	"lightdeploy.agentclient/models"
	"lightdeploy.agentclient/utils"
)

type DeployParams struct {
	ServiceName    string
	TargetDir      string
	SkipBackup     bool
	OnlyCopyFiles  bool
	HealthCheckUrl string
	FileInfos      []models.FileInfoDto
	File           io.Reader
	TempDir        string
	BackupDir      string
}

func DeployService(params DeployParams) error {
	log.Info("开始部署服务: %v", params.ServiceName)
	// 1. 解压文件到临时目录
	if err := extractZipFile(params.File, params.TempDir); err != nil {
		return fmt.Errorf("解压文件失败: %v", err)
	}

	// 2. 如果没有指定目标目录且不是仅复制文件，则需要获取服务路径
	targetDir := params.TargetDir
	if targetDir == "" && !params.OnlyCopyFiles {
		log.Info("获取服务路径: %v", params.ServiceName)
		dir, err := utils.GetWindowsServiceLocation(params.ServiceName)
		if err != nil {
			return fmt.Errorf("获取服务路径失败: %v", err)
		}
		targetDir = dir
	}

	// 3. 如果需要备份，执行备份操作
	if !params.SkipBackup {
		log.Info("开始备份文件: %v", params.ServiceName)
		if err := backupFiles(targetDir, params.BackupDir, params.FileInfos); err != nil {
			return fmt.Errorf("备份文件失败: %v", err)
		}
	}

	// 4. 如果不是仅复制文件，则停止服务
	if !params.OnlyCopyFiles {
		log.Info("停止服务: %v", params.ServiceName)
		if err := utils.StopService(params.ServiceName); err != nil {
			fmt.Println("停止服务失败: %v", err)
		}
	}

	// 5. 复制文件
	maxRetries := 3
	var lastErr error
	for i := 0; i < maxRetries; i++ {
		log.Info("复制文件: %v", params.ServiceName)
		if err := copyFiles(params.TempDir, targetDir); err != nil {
			lastErr = err
			time.Sleep(time.Second * 3)
			continue
		}
		lastErr = nil
		break
	}
	if lastErr != nil {
		return fmt.Errorf("复制文件失败: %v", lastErr)
	}

	// 6. 如果不是仅复制文件，则启动服务
	if !params.OnlyCopyFiles {
		log.Info("启动服务: %v", params.ServiceName)
		if err := utils.StartService(params.ServiceName); err != nil {
			// 如果启动失败且有备份，执行回滚
			if !params.SkipBackup {
				_ = utils.StopService(params.ServiceName)
				_ = restoreFiles(targetDir, params.BackupDir)
				_ = utils.StartService(params.ServiceName)
			}
			fmt.Println("启动服务失败: %v", err)
		}
	}

	// 7. 执行健康检查
	if params.HealthCheckUrl != "" {
		log.Info("执行健康检查: %v", params.ServiceName)
		healthy := false
		for i := 0; i < 10; i++ {
			if ok, _ := healthCheck(params.HealthCheckUrl); ok {
				healthy = true
				break
			}
			time.Sleep(time.Second * 3)
		}

		if !healthy {
			if !params.SkipBackup {
				// 执行回滚
				_ = utils.StopService(params.ServiceName)
				_ = restoreFiles(targetDir, params.BackupDir)
				_ = utils.StartService(params.ServiceName)
				return fmt.Errorf("健康检查失败，已回滚")
			}
			return fmt.Errorf("健康检查失败，未备份无法回滚")
		}
	}

	// 8. 更新文件记录
	log.Info("更新文件记录: %v", params.ServiceName)
	if err := updateFileRecords(params.ServiceName, params.FileInfos); err != nil {
		return fmt.Errorf("更新文件记录失败: %v", err)
	}
	log.Info("部署服务完成: %v", params.ServiceName)
	return nil
}

func CompareFileInfos(serviceName string, fileInfos []models.FileInfoDto) ([]models.FileInfoDto, error) {
	targetDir, err := utils.GetWindowsServiceLocation(serviceName)
	if err != nil {
		return nil, fmt.Errorf("获取服务路径失败: %v", err)
	}

	existingRecords, err := models.GetFileRecordsByServiceName(serviceName)
	if err != nil {
		return nil, fmt.Errorf("获取现有记录失败: %v", err)
	}

	// 创建map用于快速查找现有记录
	existingMap := make(map[string]models.FileRecord)
	for _, record := range existingRecords {
		key := filepath.Join(record.RelativeDirectory, record.FileName)
		existingMap[key] = record
	}

	// 比较文件并找出需要更新的文件
	var changedFiles []models.FileInfoDto
	for _, newFile := range fileInfos {
		localPath := filepath.Join(targetDir, newFile.RelativeDirectory, newFile.FileName)

		// 检查本地文件是否存在
		fileInfo, err := os.Stat(localPath)
		if err != nil {
			// 文件不存在，加入变更列表
			changedFiles = append(changedFiles, newFile)
			continue
		}

		// 比较文件大小
		if fileInfo.Size() != newFile.FileSize {
			changedFiles = append(changedFiles, newFile)
			continue
		}

		// 文件大小相同时，检查数据库记录
		key := filepath.Join(newFile.RelativeDirectory, newFile.FileName)
		if existingRecord, exists := existingMap[key]; exists {
			// 存在记录且MD5不同，加入变更列表
			if existingRecord.MD5 != newFile.MD5 {
				changedFiles = append(changedFiles, newFile)
			}
		} else {
			// 没有数据库记录，加入变更列表
			changedFiles = append(changedFiles, newFile)
		}
	}

	return changedFiles, nil
}

func extractZipFile(reader io.Reader, destDir string) error {
	// 创建临时文件保存上传的zip
	tempFile, err := os.CreateTemp("", "upload*.zip")
	if err != nil {
		return err
	}
	defer os.Remove(tempFile.Name())
	defer tempFile.Close()

	// 复制上传的内容到临时文件
	if _, err := io.Copy(tempFile, reader); err != nil {
		return err
	}

	// 打开zip文件
	zipReader, err := zip.OpenReader(tempFile.Name())
	if err != nil {
		return err
	}
	defer zipReader.Close()

	// 解压文件
	for _, file := range zipReader.File {
		path := filepath.Join(destDir, file.Name)

		if file.FileInfo().IsDir() {
			os.MkdirAll(path, os.ModePerm)
			continue
		}

		if err := os.MkdirAll(filepath.Dir(path), os.ModePerm); err != nil {
			return err
		}

		dstFile, err := os.OpenFile(path, os.O_WRONLY|os.O_CREATE|os.O_TRUNC, file.Mode())
		if err != nil {
			return err
		}

		srcFile, err := file.Open()
		if err != nil {
			dstFile.Close()
			return err
		}

		_, err = io.Copy(dstFile, srcFile)
		srcFile.Close()
		dstFile.Close()

		if err != nil {
			return err
		}
	}
	return nil
}

func backupFiles(sourceDir, backupDir string, fileInfos []models.FileInfoDto) error {
	for _, fileInfo := range fileInfos {
		srcPath := filepath.Join(sourceDir, fileInfo.RelativeDirectory, fileInfo.FileName)
		dstPath := filepath.Join(backupDir, fileInfo.RelativeDirectory, fileInfo.FileName)

		if err := os.MkdirAll(filepath.Dir(dstPath), os.ModePerm); err != nil {
			return err
		}

		if err := utils.CopyFile(srcPath, dstPath); err != nil && !os.IsNotExist(err) {
			return err
		}
	}
	return nil
}

func restoreFiles(targetDir, backupDir string) error {
	return filepath.Walk(backupDir, func(path string, info os.FileInfo, err error) error {
		if err != nil {
			return err
		}

		relPath, err := filepath.Rel(backupDir, path)
		if err != nil {
			return err
		}

		targetPath := filepath.Join(targetDir, relPath)

		if info.IsDir() {
			return os.MkdirAll(targetPath, info.Mode())
		}

		return utils.CopyFile(path, targetPath)
	})
}

func copyFiles(sourceDir, targetDir string) error {
	return filepath.Walk(sourceDir, func(path string, info os.FileInfo, err error) error {
		if err != nil {
			return err
		}

		relPath, err := filepath.Rel(sourceDir, path)
		if err != nil {
			return err
		}

		targetPath := filepath.Join(targetDir, relPath)

		if info.IsDir() {
			return os.MkdirAll(targetPath, info.Mode())
		}

		return utils.CopyFile(path, targetPath)
	})
}

func healthCheck(url string) (bool, error) {
	resp, err := http.Get(url)
	if err != nil {
		return false, err
	}
	defer resp.Body.Close()

	return resp.StatusCode == http.StatusOK, nil
}

func updateFileRecords(serviceName string, fileInfos []models.FileInfoDto) error {
	// 如果文件信息为空则直接返回
	if len(fileInfos) == 0 {
		return nil
	}

	// 获取当前时间戳
	timestamp := time.Now().UnixNano()

	// 开启事务
	tx, err := db.DB.Begin()
	if err != nil {
		return fmt.Errorf("开启事务失败: %v", err)
	}
	defer func() {
		if r := recover(); r != nil {
			tx.Rollback()
		}
	}()

	for _, fileInfo := range fileInfos {
		// 使用 Upsert 语法
		_, err := tx.Exec(`
			INSERT INTO file_records (
				service_name, publish_timestamp, absolute_directory, 
				relative_directory, file_name, md5
			) VALUES (?, ?, ?, ?, ?, ?)
			ON CONFLICT (service_name, relative_directory, file_name) 
			DO UPDATE SET 
				publish_timestamp = ?,
				absolute_directory = ?,
				md5 = ?
		`,
			serviceName, timestamp, fileInfo.AbsoluteDirectory,
			fileInfo.RelativeDirectory, fileInfo.FileName, fileInfo.MD5,
			// UPDATE 部分的参数
			timestamp, fileInfo.AbsoluteDirectory, fileInfo.MD5,
		)

		if err != nil {
			tx.Rollback()
			return fmt.Errorf("更新记录失败: %v", err)
		}
	}

	if err := tx.Commit(); err != nil {
		return fmt.Errorf("提交事务失败: %v", err)
	}

	return nil
}
