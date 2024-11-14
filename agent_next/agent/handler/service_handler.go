package handler

import (
	"bytes"
	"io"
	"net/http"
	"path/filepath"

	"lightdeploy.agentclient/models"
	"lightdeploy.agentclient/services"
	"lightdeploy.agentclient/utils"
)

// CheckServiceExistHandler 检查服务是否存在
func CheckServiceExistHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		http.Error(w, "Method not allowed", http.StatusMethodNotAllowed)
		return
	}

	serviceName := r.URL.Query().Get("serviceName")
	exist := utils.ServiceIsExisted(serviceName)
	utils.WriteSuccessJSON(w, exist)
}

// InstallWindowsServiceHandler 安装Windows服务
func InstallWindowsServiceHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		http.Error(w, "Method not allowed", http.StatusMethodNotAllowed)
		return
	}

	// 先解析multipart表单
	if err := r.ParseMultipartForm(32 << 20); err != nil { // 32MB 最大内存
		utils.WriteErrorJSON(w, utils.ApiResponse{
			Code:    utils.API_ERROR_CODE,
			Message: "解析表单失败: " + err.Error(),
		})
		return
	}

	// 获取文件
	file, _, err := r.FormFile("file")
	if err != nil {
		utils.WriteErrorJSON(w, utils.ApiResponse{
			Code:    utils.API_ERROR_CODE,
			Message: "获取上传文件失败: " + err.Error(),
		})
		return
	}
	defer file.Close()

	// 解析其他表单数据
	var dto models.InstallWindowsServiceDto
	dto.ServiceName = r.FormValue("serviceName")
	dto.ExeFullPath = r.FormValue("exeFullPath")
	dto.Params = r.FormValue("params")
	dto.ServiceDescription = r.FormValue("serviceDescription")
	dto.File, err = io.ReadAll(file)
	if err != nil {
		utils.WriteErrorJSON(w, utils.ApiResponse{
			Code:    utils.API_ERROR_CODE,
			Message: "读取文件失败: " + err.Error(),
		})
		return
	}
	if exist := utils.ServiceIsExisted(dto.ServiceName); exist {
		utils.WriteErrorJSON(w, utils.ApiResponse{
			Code:    utils.API_ERROR_CODE,
			Message: "服务已存在",
		})
		return
	}

	// 检查目标目录
	targetDir := filepath.Dir(dto.ExeFullPath)
	if err := services.ExtractZipFile(bytes.NewReader(dto.File), targetDir); err != nil {
		utils.WriteErrorJSON(w, utils.ApiResponse{
			Code:    utils.API_ERROR_CODE,
			Message: "解压文件失败: " + err.Error(),
		})
		return
	}

	if err := services.ExtractZipFile(bytes.NewReader(dto.File), targetDir); err != nil {
		utils.WriteErrorJSON(w, utils.ApiResponse{
			Code:    utils.API_ERROR_CODE,
			Message: "解压文件失败: " + err.Error(),
		})
		return
	}

	// 继续安装服务
	if err := utils.InstallWindowsService(dto); err != nil {
		utils.WriteErrorJSON(w, utils.ApiResponse{
			Code:    utils.API_ERROR_CODE,
			Message: "安装失败: " + err.Error(),
		})
		return
	}

	utils.WriteSuccessJSON(w, nil)
}

// StartServiceHandler 启动服务
func StartServiceHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		http.Error(w, "Method not allowed", http.StatusMethodNotAllowed)
		return
	}

	serviceName := r.URL.Query().Get("serviceName")
	if !utils.ServiceIsExisted(serviceName) {
		utils.WriteErrorJSON(w, utils.ApiResponse{
			Code:    utils.API_ERROR_CODE,
			Message: "服务不存在",
		})
		return
	}

	if err := utils.StartService(serviceName); err != nil {
		utils.WriteErrorJSON(w, utils.ApiResponse{
			Code:    utils.API_ERROR_CODE,
			Message: err.Error(),
		})
		return
	}

	utils.WriteSuccessJSON(w, nil)
}

// StopServiceHandler 停止服务
func StopServiceHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		http.Error(w, "Method not allowed", http.StatusMethodNotAllowed)
		return
	}

	serviceName := r.URL.Query().Get("serviceName")
	if !utils.ServiceIsExisted(serviceName) {
		utils.WriteErrorJSON(w, utils.ApiResponse{
			Code:    utils.API_ERROR_CODE,
			Message: "服务不存在",
		})
		return
	}

	if err := utils.StopService(serviceName); err != nil {
		utils.WriteErrorJSON(w, utils.ApiResponse{
			Code:    utils.API_ERROR_CODE,
			Message: err.Error(),
		})
		return
	}

	utils.WriteSuccessJSON(w, nil)
}

// GetStatusHandler 获取服务状态
func GetStatusHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodGet {
		http.Error(w, "Method not allowed", http.StatusMethodNotAllowed)
		return
	}

	serviceName := r.URL.Query().Get("serviceName")
	if !utils.ServiceIsExisted(serviceName) {
		utils.WriteErrorJSON(w, utils.ApiResponse{
			Code:    utils.API_ERROR_CODE,
			Message: "服务不存在",
		})
		return
	}

	status := utils.GetStatus(serviceName)
	utils.WriteSuccessJSON(w, status)
}
