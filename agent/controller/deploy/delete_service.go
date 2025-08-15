package deploy

import (
	"ld_agent/internal/windows_service"
	"ld_shared/error_response"
	"net/http"

	"github.com/gin-gonic/gin"
)

// 删除Windows服务接口
func DeleteService(c *gin.Context) {
	serviceName := c.Query("serviceName")
	if serviceName == "" {
		error_response.NewErrorResponse(c, "serviceName is required")
		return
	}

	err := windows_service.DeleteService(serviceName)
	if err != nil {
		error_response.NewErrorResponse(c, "删除服务失败: "+err.Error())
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"message": "服务删除成功",
	})
}
