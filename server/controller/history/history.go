package history

import (
	"fmt"
	"net/http"
	"strconv"
	"time"

	"ld_server/db"
	"ld_server/model"
	"ld_shared/clog"
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
		Desc("deploy_time"). // 按部署时间倒序排序
		Limit(10).           // 限制最多10个
		Find(&histories)
	if err != nil {
		error_response.NewErrorResponse(c, "查询部署历史记录失败: "+err.Error())
		return
	}

	c.JSON(http.StatusOK, histories)
}

type HistoryResponse struct {
	Id          int       `json:"id"`
	ServiceId   int       `json:"serviceId"`
	ServiceName string    `json:"serviceName"`
	DeployTime  time.Time `json:"deployTime"`
	Comment     string    `json:"comment"`
	Environment string    `json:"environment"`
}

func GetHistoryPageList(c *gin.Context) {
	page := c.Query("page")
	pageSize := c.Query("pageSize")
	serviceId := c.Query("serviceId")

	pageInt, err := strconv.Atoi(page)
	if err != nil {
		error_response.NewErrorResponse(c, "invalid page")
		return
	}

	pageSizeInt, err := strconv.Atoi(pageSize)
	if err != nil {
		error_response.NewErrorResponse(c, "invalid pageSize")
		return
	}

	offset := (pageInt - 1) * pageSizeInt

	var histories []HistoryResponse
	query := db.Engine.Table("deploy_history").
		Alias("h").
		Join("LEFT", []string{"deploy_service", "s"}, "h.service_id = s.id").
		Select("h.id, h.service_id, s.service_name, h.deploy_time, h.comment, s.environment").
		Desc("h.deploy_time").
		Limit(pageSizeInt, offset)
	serviceIdInt, _ := strconv.Atoi(serviceId)
	clog.GetContextLogger(c).Info(fmt.Sprintf("serviceIdInt: %d", serviceIdInt))
	if serviceIdInt > 0 {
		query = query.Where("h.service_id = ?", serviceIdInt)
	}
	err = query.Find(&histories)
	if err != nil {
		error_response.NewErrorResponse(c, "查询部署历史记录失败: "+err.Error())
		return
	}

	var total int64
	total, err = db.Engine.Table("deploy_history").Count()
	if err != nil {
		error_response.NewErrorResponse(c, "查询部署历史记录总数失败: "+err.Error())
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"data":  histories,
		"total": total,
	})
}
