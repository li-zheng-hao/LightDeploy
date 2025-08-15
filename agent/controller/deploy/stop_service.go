package deploy

import (
	"ld_agent/internal/windows_service"
	"ld_shared/error_response"
	"net/http"

	"log/slog"

	"github.com/gin-gonic/gin"
)

func StopService(c *gin.Context) {
	slog.Info("开始停止服务")
	serviceName := c.Query("serviceName")
	if serviceName == "" {
		error_response.NewErrorResponse(c, "服务名称不能为空")
		return
	}
	err := windows_service.StopService(serviceName)
	if err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}
	c.JSON(http.StatusOK, nil)
}
