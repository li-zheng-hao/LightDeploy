package agent

import (
	"ld_server/db"
	"ld_server/model"
	"ld_shared/error_response"
	"net/http"

	"github.com/gin-gonic/gin"
)

func GetAllAgent(c *gin.Context) {
	// 从数据库中获取所有目标机器信息，按host去重
	var targets []model.DeployTarget
	if err := db.DB.Select("DISTINCT ON (host) *").Find(&targets).Error; err != nil {
		error_response.NewErrorResponse(c, "获取目标机器列表失败: "+err.Error())
		return
	}

	c.JSON(http.StatusOK, targets)
}
