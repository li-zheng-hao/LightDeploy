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

	"github.com/gofiber/fiber/v2"
)

type UpdateAgentRequest struct {
	File             *multipart.FileHeader `form:"file"`
	AgentServiceName string                `form:"agentServiceName"`
	TargetId         int                   `form:"targetId"`
}

func UpdateAgent(c *fiber.Ctx) error {
	// 获取上传的文件
	file, err := c.FormFile("file")
	if err != nil {
		return error_response.NewErrorResponse(c, "请上传更新文件")
	}

	// 获取其他表单参数
	agentServiceName := c.FormValue("agentServiceName")
	if agentServiceName == "" {
		return error_response.NewErrorResponse(c, "请提供代理服务名称")
	}

	targetIdStr := c.FormValue("targetId")
	if targetIdStr == "" {
		return error_response.NewErrorResponse(c, "请提供目标机器ID")
	}

	// 解析目标ID
	var targetId int
	if _, err := fmt.Sscanf(targetIdStr, "%d", &targetId); err != nil {
		return error_response.NewErrorResponse(c, "目标机器ID格式错误")
	}

	// 打开上传的文件
	uploadedFile, err := file.Open()
	if err != nil {
		return error_response.NewErrorResponse(c, "打开文件失败: "+err.Error())
	}
	defer uploadedFile.Close()

	// 获取目标机器信息
	var target model.DeployTarget
	if err := db.DB.First(&target, targetId).Error; err != nil {
		return error_response.NewErrorResponse(c, "目标机器不存在")
	}

	req, err := http.NewRequest("POST", fmt.Sprintf("http://%s:%d/api/version/update", target.Host, target.Port), nil)
	if err != nil {
		return error_response.NewErrorResponse(c, "创建HTTP请求失败: "+err.Error())
	}

	body := &bytes.Buffer{}
	writer := multipart.NewWriter(body)
	part, err := writer.CreateFormFile("file", file.Filename)
	if err != nil {
		return error_response.NewErrorResponse(c, "创建表单失败: "+err.Error())
	}

	_, err = io.Copy(part, uploadedFile)
	if err != nil {
		return error_response.NewErrorResponse(c, "复制文件失败: "+err.Error())
	}

	writer.WriteField("agentServiceName", agentServiceName)
	writer.Close()

	req.Body = io.NopCloser(body)
	req.Header.Set("Content-Type", writer.FormDataContentType())

	client := &http.Client{}
	resp, err := client.Do(req)
	if err != nil {
		return error_response.NewErrorResponse(c, "发送更新请求失败: "+err.Error())
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		bodyBytes, _ := io.ReadAll(resp.Body)
		return error_response.NewErrorResponse(c, fmt.Sprintf("更新失败，状态码：%d，错误：%s", resp.StatusCode, string(bodyBytes)))
	}

	return c.JSON(fiber.Map{
		"message": "更新指令已发送",
	})
}
