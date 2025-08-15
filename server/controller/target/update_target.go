package target

import (
	"ld_server/db"
	"ld_server/model"
	"ld_shared/error_response"
	"net/http"

	"github.com/gin-gonic/gin"
)

// UpdateTargetRequest 更新部署目标的请求结构
type UpdateTargetRequest struct {
	Id          int    `json:"id" binding:"required"`
	ServiceId   int    `json:"serviceId"`
	Host        string `json:"host"`
	Port        int    `json:"port"`
	SecretKey   string `json:"secretKey"`
	ServicePath string `json:"servicePath"`
	Comment     string `json:"comment"`
	ExePath     string `json:"exePath"`
	ExeParams   string `json:"exeParams"`
}

// SuccessResponse 成功响应结构
type SuccessResponse struct {
	Message string `json:"message"`
}

// UpdateTarget 更新部署目标
func UpdateTarget(c *gin.Context) {
	var req UpdateTargetRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		error_response.NewErrorResponse(c, "参数无效")
		return
	}

	// 验证记录是否存在
	var existingTarget model.DeployTarget
	if err := db.DB.First(&existingTarget, req.Id).Error; err != nil {
		error_response.NewErrorResponse(c, "部署目标不存在")
		return
	}

	// 构建更新对象
	target := model.DeployTarget{
		Id:          req.Id,
		ServiceId:   req.ServiceId,
		Host:        req.Host,
		Port:        req.Port,
		SecretKey:   req.SecretKey,
		ServicePath: req.ServicePath,
		Comment:     req.Comment,
		ExePath:     req.ExePath,
		ExeParams:   req.ExeParams,
	}

	// 更新记录
	if err := db.DB.Save(&target).Error; err != nil {
		error_response.NewErrorResponse(c, "更新部署目标失败")
		return
	}

	c.JSON(http.StatusOK, SuccessResponse{Message: "更新成功"})
}
