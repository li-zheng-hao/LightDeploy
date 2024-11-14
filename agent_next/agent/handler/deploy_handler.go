package handler

import (
	"encoding/json"
	"io"
	"net/http"
	"os"
	"path"
	"path/filepath"

	"lightdeploy.agentclient/log"
	"lightdeploy.agentclient/models"
	"lightdeploy.agentclient/services"
	"lightdeploy.agentclient/utils"
)

// DeployHandler 部署
func DeployHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		http.Error(w, "Method not allowed", http.StatusMethodNotAllowed)
		return
	}

	// 解析multipart表单
	err := r.ParseMultipartForm(2048 << 20) // 2048MB
	if err != nil {
		utils.WriteErrorJSON(w, utils.ApiResponse{
			Code:    utils.API_ERROR_CODE,
			Message: err.Error(),
		})
		return
	}

	// 获取表单参数
	serviceName := r.FormValue("serviceName")
	targetDir := r.FormValue("targetDir")
	skipBackup := r.FormValue("skipBackup") == "true"
	onlyCopyFiles := r.FormValue("onlyCopyFiles") == "true"
	healthCheckUrl := r.FormValue("healthCheckUrl")

	// 获取文件信息
	fileInfosJson := r.FormValue("fileInfos")
	var fileInfos []models.FileInfoDto
	if err := json.Unmarshal([]byte(fileInfosJson), &fileInfos); err != nil {
		utils.WriteErrorJSON(w, utils.ApiResponse{
			Code:    utils.API_ERROR_CODE,
			Message: "解析文件信息失败: " + err.Error(),
		})
		return
	}

	// 获取上传的文件
	file, _, err := r.FormFile("file")
	if err != nil {
		utils.WriteErrorJSON(w, utils.ApiResponse{
			Code:    utils.API_ERROR_CODE,
			Message: "获取上传文件失败: " + err.Error(),
		})
		return
	}
	defer file.Close()

	// 创建临时目录
	tempDir, err := os.MkdirTemp("", "deploy_*")
	if err != nil {
		utils.WriteErrorJSON(w, utils.ApiResponse{
			Code:    utils.API_ERROR_CODE,
			Message: "创建临时目录失败: " + err.Error(),
		})
		return
	}
	defer os.RemoveAll(tempDir)

	backupDir, err := os.MkdirTemp("", "backup_*")
	if err != nil {
		utils.WriteErrorJSON(w, utils.ApiResponse{
			Code:    utils.API_ERROR_CODE,
			Message: "创建备份目录失败: " + err.Error(),
		})
		return
	}
	defer os.RemoveAll(backupDir)

	// 部署服务
	err = services.DeployService(services.DeployParams{
		ServiceName:    serviceName,
		TargetDir:      targetDir,
		SkipBackup:     skipBackup,
		OnlyCopyFiles:  onlyCopyFiles,
		HealthCheckUrl: healthCheckUrl,
		FileInfos:      fileInfos,
		File:           file,
		TempDir:        tempDir,
		BackupDir:      backupDir,
	})

	if err != nil {
		utils.WriteErrorJSON(w, utils.ApiResponse{
			Code:    utils.API_ERROR_CODE,
			Message: err.Error(),
		})
		return
	}

	utils.WriteSuccessJSON(w, nil)
}

// CompareDirHandler 比较目录
func CompareDirHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		http.Error(w, "Method not allowed", http.StatusMethodNotAllowed)
		return
	}

	// 获取目录参数和文件信息
	dir := r.URL.Query().Get("dir")
	var fileInfos []models.FileInfoDto
	if err := json.NewDecoder(r.Body).Decode(&fileInfos); err != nil {
		utils.WriteErrorJSON(w, utils.ApiResponse{
			Code:    utils.API_ERROR_CODE,
			Message: err.Error(),
		})
		return
	}

	// 检查目录是否存在
	if _, err := os.Stat(dir); os.IsNotExist(err) {
		utils.WriteErrorJSON(w, utils.ApiResponse{
			Code:    utils.API_ERROR_CODE,
			Message: "目录不存在",
		})
		return
	}

	// 比较文件信息
	var result []models.FileInfoDto
	for _, fileInfo := range fileInfos {
		filePath := filepath.Join(dir, fileInfo.RelativeDirectory, fileInfo.FileName)

		if _, err := os.Stat(filePath); os.IsNotExist(err) {
			result = append(result, fileInfo)
			continue
		}

		fi, err := os.Stat(filePath)
		if err != nil {
			utils.WriteErrorJSON(w, utils.ApiResponse{
				Code:    utils.API_ERROR_CODE,
				Message: err.Error(),
			})
			return
		}

		if fi.Size() != fileInfo.FileSize {
			result = append(result, fileInfo)
			continue
		}

		md5, err := utils.GetFileMd5(filePath, []string{".log", ".db", ".db-shm", ".db-wal"})
		if err != nil {
			utils.WriteErrorJSON(w, utils.ApiResponse{
				Code:    utils.API_ERROR_CODE,
				Message: err.Error(),
			})
			return
		}
		if md5 != fileInfo.MD5 {
			result = append(result, fileInfo)
		}
	}

	utils.WriteSuccessJSON(w, result)
}

// CompareHandler 比较文件
func CompareHandler(w http.ResponseWriter, r *http.Request) {
	log.Info("开始比较文件")
	if r.Method != http.MethodPost {
		http.Error(w, "Method not allowed", http.StatusMethodNotAllowed)
		return
	}

	serviceName := r.URL.Query().Get("serviceName")
	var fileInfos []models.FileInfoDto
	if err := json.NewDecoder(r.Body).Decode(&fileInfos); err != nil {
		utils.WriteErrorJSON(w, utils.ApiResponse{
			Code:    utils.API_ERROR_CODE,
			Message: err.Error(),
		})
		return
	}

	newFileInfos, err := services.CompareFileInfos(serviceName, fileInfos)
	if err != nil {
		utils.WriteErrorJSON(w, utils.ApiResponse{
			Code:    utils.API_ERROR_CODE,
			Message: err.Error(),
		})
		return
	}
	log.Info("比较文件完成")
	utils.WriteSuccessJSON(w, newFileInfos)
}

// PingHandler 心跳检测
func PingHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodGet {
		http.Error(w, "Method not allowed", http.StatusMethodNotAllowed)
		return
	}
	utils.WriteSuccessJSON(w, utils.ApiResponse{
		Code: utils.API_SUCCESS_CODE,
	})
}

// UpdateSelfHandler 更新自身
func UpdateSelfHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		http.Error(w, "Method not allowed", http.StatusMethodNotAllowed)
		return
	}

	file, header, err := r.FormFile("file")
	if err != nil {
		http.Error(w, err.Error(), http.StatusBadRequest)
		return
	}
	defer file.Close()

	// 获取更新包存放路径
	execPath, _ := os.Executable()
	parentDir := path.Dir(path.Dir(execPath))
	targetPath := path.Join(parentDir, "LightDeployUpdateService", "UpdatePackages")

	if err := os.MkdirAll(targetPath, 0755); err != nil {
		utils.WriteErrorJSON(w, utils.ApiResponse{
			Code:    utils.API_ERROR_CODE,
			Message: err.Error(),
		})
		return
	}

	targetFile := path.Join(targetPath, header.Filename)
	if _, err := os.Stat(targetFile); err == nil {
		utils.WriteErrorJSON(w, utils.ApiResponse{
			Code:    utils.API_ERROR_CODE,
			Message: "更新文件已存在,请稍后再试",
		})
		return
	}

	dst, err := os.Create(targetFile)
	if err != nil {
		utils.WriteErrorJSON(w, utils.ApiResponse{
			Code:    utils.API_ERROR_CODE,
			Message: err.Error(),
		})
		return
	}
	defer dst.Close()

	if _, err := io.Copy(dst, file); err != nil {
		utils.WriteErrorJSON(w, utils.ApiResponse{
			Code:    utils.API_ERROR_CODE,
			Message: err.Error(),
		})
		return
	}

	utils.WriteSuccessJSON(w, nil)
}
