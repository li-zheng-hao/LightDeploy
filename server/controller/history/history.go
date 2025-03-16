package history

import (
	"net/http"
	"strconv"

	"ld_server/db"
	"ld_server/model"
	"ld_shared/error_response"

	"github.com/gin-gonic/gin"
)

func GetHistory(c *gin.Context) {
	serviceId := c.Param("serviceId")
	if serviceId == "" {
		error_response.NewErrorResponse(c, "serviceId is required")
		return
	}

	// 将 serviceId 转换为整数
	sId, err := strconv.Atoi(serviceId)
	if err != nil {
		error_response.NewErrorResponse(c, "invalid serviceId")
		return
	}

	// 查询部署历史记录，限制最多10个
	var histories []model.DeployHistory
	err = db.Engine.Where("service_id = ?", sId).
		Desc("deploy_time").  // 按部署时间倒序排序
		Limit(10).            // 限制最多10个
		Find(&histories)
	if err != nil {
		error_response.NewErrorResponse(c, "查询部署历史记录失败: "+err.Error())
		return
	}

	c.JSON(http.StatusOK, histories)
}