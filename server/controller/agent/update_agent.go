package agent

import (
	"bytes"
	"fmt"
	"io"
	"ld_server/db"
	"ld_server/model"
	"ld_shared/error_response"
	"mime/multipart"
	"net/http"

	"github.com/gin-gonic/gin"
)

type UpdateAgentRequest struct {
	File             *multipart.FileHeader `form:"file"`
	AgentServiceName string                `form:"agentServiceName"`
	TargetId         int                   `form:"targetId"`
}

func UpdateAgent(c *gin.Context) {
	// 解析请求参数
	var request UpdateAgentRequest
	if err := c.ShouldBind(&request); err != nil {
		error_response.NewErrorResponse(c, err.Error())
		return
	}

	if request.File == nil {
		error_response.NewErrorResponse(c, "请上传更新文件")
		return
	}

	// 获取目标机器信息
	var target model.DeployTarget
	if err := db.DB.First(&target, request.TargetId).Error; err != nil {
		error_response.NewErrorResponse(c, "目标机器不存在")
		return
	}

	// 上传新的agent文件到目标机器
	file, err := request.File.Open()
	if err != nil {
		error_response.NewErrorResponse(c, "打开文件失败: "+err.Error())
		return
	}
	defer file.Close()

	req, err := http.NewRequest("POST", fmt.Sprintf("http://%s:%d/api/version/update", target.Host, target.Port), nil)
	if err != nil {
		error_response.NewErrorResponse(c, "创建HTTP请求失败: "+err.Error())
		return
	}

	body := &bytes.Buffer{}
	writer := multipart.NewWriter(body)
	part, err := writer.CreateFormFile("file", request.File.Filename)
	if err != nil {
		error_response.NewErrorResponse(c, "创建表单失败: "+err.Error())
		return
	}

	_, err = io.Copy(part, file)
	if err != nil {
		error_response.NewErrorResponse(c, "复制文件失败: "+err.Error())
		return
	}

	writer.WriteField("agentServiceName", request.AgentServiceName)
	writer.Close()

	req.Body = io.NopCloser(body)
	req.Header.Set("Content-Type", writer.FormDataContentType())

	client := &http.Client{}
	resp, err := client.Do(req)
	if err != nil {
		error_response.NewErrorResponse(c, "发送更新请求失败: "+err.Error())
		return
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		bodyBytes, _ := io.ReadAll(resp.Body)
		error_response.NewErrorResponse(c, fmt.Sprintf("更新失败，状态码：%d，错误：%s", resp.StatusCode, string(bodyBytes)))
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"message": "更新指令已发送",
	})
}
