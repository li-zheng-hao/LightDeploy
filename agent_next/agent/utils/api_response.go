package utils

import (
	"encoding/json"
	"net/http"
)

const (
	API_SUCCESS_CODE = 200 // 成功
	API_ERROR_CODE   = 500 // 错误
)

type ApiResponse struct {
	Code    int         `json:"code"`
	Message string      `json:"msg"`
	Data    interface{} `json:"data"`
	Success bool        `json:"success"`
}

func WriteJSON(w http.ResponseWriter, statusCode int, data interface{}) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(statusCode)
	json.NewEncoder(w).Encode(data)
}

func WriteErrorJSON(w http.ResponseWriter, apiResponse ApiResponse) {
	WriteJSON(w, http.StatusOK, apiResponse)
}
func WriteErrorJsonWithStatus(w http.ResponseWriter, statusCode int, apiResponse ApiResponse) {
	WriteJSON(w, statusCode, apiResponse)
}

func WriteSuccessJSON(w http.ResponseWriter, data interface{}) {
	WriteJSON(w, http.StatusOK, ApiResponse{
		Code:    API_SUCCESS_CODE,
		Data:    data,
		Success: true,
	})
}
