package target

import (
	"ld_server/db"
	"ld_server/model"
	"ld_shared/error_response"
	"net/http"

	"github.com/gin-gonic/gin"
)

// ListTargets 获取部署目标列表
func ListTargets(c *gin.Context) {
	serviceId := c.Query("serviceId")
	var targets []model.DeployTarget
	if serviceId != "" {
		if err := db.DB.Where("service_id = ?", serviceId).Find(&targets).Error; err != nil {
			error_response.NewErrorResponse(c, "获取部署目标列表失败")
			return
		}
	} else {
		if err := db.DB.Find(&targets).Error; err != nil {
			error_response.NewErrorResponse(c, "获取部署目标列表失败")
			return
		}
	}

	c.JSON(http.StatusOK, targets)
}
