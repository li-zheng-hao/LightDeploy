package history

import (
	"fmt"
	"strconv"
	"time"

	"ld_server/db"
	"ld_shared/clog"
	"ld_shared/error_response"

	"github.com/gofiber/fiber/v2"
)

type HistoryResponse struct {
	Id          int       `json:"id"`
	ServiceId   int       `json:"serviceId"`
	ServiceName string    `json:"serviceName"`
	DeployTime  time.Time `json:"deployTime"`
	Comment     string    `json:"comment"`
	Environment string    `json:"environment"`
}

func GetHistoryPageList(c *fiber.Ctx) error {
	page := c.Query("page")
	pageSize := c.Query("pageSize")
	serviceId := c.Query("serviceId")

	pageInt, err := strconv.Atoi(page)
	if err != nil {
		return error_response.NewErrorResponse(c, "invalid page")
	}

	pageSizeInt, err := strconv.Atoi(pageSize)
	if err != nil {
		return error_response.NewErrorResponse(c, "invalid pageSize")
	}

	offset := (pageInt - 1) * pageSizeInt

	var histories []HistoryResponse
	query := db.DB.Table("deploy_history").
		Select("deploy_history.id, deploy_history.service_id, deploy_service.service_name, deploy_history.deploy_time, deploy_history.comment, deploy_service.environment").
		Joins("LEFT JOIN deploy_service ON deploy_history.service_id = deploy_service.id").
		Order("deploy_history.deploy_time DESC").
		Offset(offset).
		Limit(pageSizeInt)

	serviceIdInt, _ := strconv.Atoi(serviceId)
	clog.GetFiberContextLogger(c).Info(fmt.Sprintf("serviceIdInt: %d", serviceIdInt))
	if serviceIdInt > 0 {
		query = query.Where("deploy_history.service_id = ?", serviceIdInt)
	}

	if err := query.Find(&histories).Error; err != nil {
		return error_response.NewErrorResponse(c, "查询部署历史记录失败: "+err.Error())
	}

	var total int64
	if err := db.DB.Table("deploy_history").Count(&total).Error; err != nil {
		return error_response.NewErrorResponse(c, "查询部署历史记录总数失败: "+err.Error())
	}

	return c.JSON(fiber.Map{
		"data":  histories,
		"total": total,
	})
}