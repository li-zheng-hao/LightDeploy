package handler

import (
	"encoding/json"
	"net/http"

	"lightdeploy.agentclient/config"
	"lightdeploy.agentclient/services"
	"lightdeploy.agentclient/utils"
)

// GetAgentVersionHandler 获取代理版本
func GetAgentVersionHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodGet {
		http.Error(w, "Method not allowed", http.StatusMethodNotAllowed)
		return
	}

	version := config.GetConfig().Server.Version
	json.NewEncoder(w).Encode(utils.ApiResponse{
		Code: 200,
		Data: version,
	})
}

// CopyFileHandler 复制文件
func CopyFileHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		http.Error(w, "Method not allowed", http.StatusMethodNotAllowed)
		return
	}

	// 解析multipart表单
	err := r.ParseMultipartForm(500 << 20) // 500MB 最大内存
	if err != nil {
		utils.WriteErrorJSON(w, utils.ApiResponse{
			Code:    utils.API_ERROR_CODE,
			Message: "无法解析表单",
		})
		return
	}

	// 获取上传的文件
	file, _, err := r.FormFile("file")
	if err != nil {
		utils.WriteErrorJSON(w, utils.ApiResponse{
			Code:    utils.API_ERROR_CODE,
			Message: "无法获取上传文件",
		})
		return
	}
	defer file.Close()

	// 获取目标目录
	targetDir := r.FormValue("targetDir")
	if targetDir == "" {
		utils.WriteErrorJSON(w, utils.ApiResponse{
			Code:    utils.API_ERROR_CODE,
			Message: "目标目录不能为空",
		})
		return
	}

	// 解压文件
	err = services.ExtractZipFile(file, targetDir)
	if err != nil {
		utils.WriteErrorJSON(w, utils.ApiResponse{
			Code:    utils.API_ERROR_CODE,
			Message: "解压失败: " + err.Error(),
		})
		return
	}

	utils.WriteSuccessJSON(w, nil)
}
