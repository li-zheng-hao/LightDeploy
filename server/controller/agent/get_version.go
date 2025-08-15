package agent

import (
	"encoding/json"
	"fmt"
	"ld_server/db"
	"ld_server/model"
	"ld_shared/error_response"
	"net/http"

	"github.com/gin-gonic/gin"
)

func GetVersion(c *gin.Context) {
	targetId := c.Query("targetId")
	if targetId == "" {
		error_response.NewErrorResponse(c, "targetId is required")
		return
	}

	// 获取目标机器信息
	var target model.DeployTarget
	if err := db.DB.First(&target, targetId).Error; err != nil {
		error_response.NewErrorResponse(c, "目标机器不存在")
		return
	}

	// 调用agent的版本查询接口
	url := fmt.Sprintf("http://%s:%d/api/version", target.Host, target.Port)
	resp, err := http.Get(url)
	if err != nil {
		error_response.NewErrorResponse(c, "调用agent版本查询接口失败: "+err.Error())
		return
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		error_response.NewErrorResponse(c, fmt.Sprintf("agent返回错误状态码: %d", resp.StatusCode))
		return
	}

	// 读取并解析响应
	var result struct {
		Version string `json:"version"`
	}
	if err := json.NewDecoder(resp.Body).Decode(&result); err != nil {
		error_response.NewErrorResponse(c, "解析agent响应失败: "+err.Error())
		return
	}

	c.JSON(http.StatusOK, result)
}
