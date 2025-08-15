package target

import (
	"ld_server/db"
	"ld_server/model"
	"ld_shared/error_response"
	"net/http"

	"github.com/gin-gonic/gin"
)

// CreateTargetRequest 创建部署目标的请求结构
type CreateTargetRequest struct {
	ServiceId   int    `json:"serviceId" binding:"required"`
	Host        string `json:"host" binding:"required"`
	Port        int    `json:"port" binding:"required"`
	SecretKey   string `json:"secretKey"`
	ServicePath string `json:"servicePath"`
	Comment     string `json:"comment"`
	ExePath     string `json:"exePath"`
	ExeParams   string `json:"exeParams"`
}

// CreateTarget 创建部署目标
func CreateTarget(c *gin.Context) {
	var req CreateTargetRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		error_response.NewErrorResponse(c, "参数无效")
		return
	}

	// 验证必填字段
	if req.ServiceId == 0 || req.Host == "" || req.Port == 0 {
		error_response.NewErrorResponse(c, "服务ID、主机地址和端口为必填项")
		return
	}

	// 构建模型对象
	target := model.DeployTarget{
		ServiceId:   req.ServiceId,
		Host:        req.Host,
		Port:        req.Port,
		SecretKey:   req.SecretKey,
		ServicePath: req.ServicePath,
		Comment:     req.Comment,
		ExePath:     req.ExePath,
		ExeParams:   req.ExeParams,
	}

	// 插入数据库
	if err := db.DB.Create(&target).Error; err != nil {
		error_response.NewErrorResponse(c, "创建部署目标失败,"+err.Error())
		return
	}

	c.JSON(http.StatusOK, target)
}
