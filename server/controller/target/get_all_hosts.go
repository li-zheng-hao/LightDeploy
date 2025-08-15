package target

import (
	"ld_server/db"
	"ld_shared/error_response"
	"net/http"

	"github.com/gin-gonic/gin"
)

// GetAllHostsResponse 获取所有主机的响应结构
type GetAllHostsResponse struct {
	Host string `json:"host"`
}

/*
获取所有目标主机，去重
*/
func GetAllHosts(c *gin.Context) {
	var results []GetAllHostsResponse

	// 查询所有不重复的主机地址
	err := db.DB.Table("deploy_target").
		Select("DISTINCT host").
		Find(&results).Error

	if err != nil {
		error_response.NewErrorResponse(c, "获取主机列表失败: "+err.Error())
		return
	}

	c.JSON(http.StatusOK, results)
}
