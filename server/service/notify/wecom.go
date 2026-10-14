package notify

import (
	"bytes"
	"encoding/json"
	"fmt"
	"ld_shared/sse"
	"net/http"
	"strings"
	"time"
)

// 企业微信群机器人 Webhook 地址前缀
const wecomWebhookBase = "https://qyapi.weixin.qq.com/cgi-bin/webhook/send?key="

// DeployNotifyInfo 部署结果通知所需的信息
type DeployNotifyInfo struct {
	WebhookKey  string   // 机器人 key
	Environment string   // 环境
	GroupName   string   // 服务分组
	ServiceName string   // 服务名称
	Hosts       []string // 部署目标 host:port 列表
	Success     bool     // 是否部署成功
	Comment     string   // 发布说明
	// 需要@的企业微信成员 userid，多个用逗号或空格分隔（markdown 消息通过 <@userid> 语法实现）
	MentionUsers string
}

// SendDeployNotification 向企业微信机器人发送部署结果通知（markdown 类型）。
// key 为空时直接返回 nil（表示未配置，无需发送）。发送失败仅记录 SSE 日志，不影响部署流程。
func SendDeployNotification(info DeployNotifyInfo) error {
	if strings.TrimSpace(info.WebhookKey) == "" {
		sse.SendMessage("未配置企业微信机器人Key，跳过通知")
		return nil
	}

	result := "<font color=\"warning\">失败</font>"
	if info.Success {
		result = "<font color=\"info\">成功</font>"
	}

	var builder strings.Builder
	builder.WriteString("# LightDeploy 部署通知\n")
	builder.WriteString(fmt.Sprintf("> 环境：%s\n", valueOrDash(info.Environment)))
	builder.WriteString(fmt.Sprintf("> 服务分组：%s\n", valueOrDash(info.GroupName)))
	builder.WriteString(fmt.Sprintf("> 服务名称：%s\n", valueOrDash(info.ServiceName)))
	builder.WriteString(fmt.Sprintf("> 部署结果：%s\n", result))
	builder.WriteString(fmt.Sprintf("> 部署目标：%s\n", valueOrDash(strings.Join(info.Hosts, ", "))))
	builder.WriteString(fmt.Sprintf("> 发布说明：%s\n", valueOrDash(info.Comment)))

	// 企业微信 markdown 消息通过 <@userid> 语法@成员
	if mentions := buildMentions(info.MentionUsers); mentions != "" {
		builder.WriteString("\n")
		builder.WriteString(mentions)
		builder.WriteString("\n")
	}

	payload := map[string]interface{}{
		"msgtype":  "markdown",
		"markdown": map[string]string{"content": builder.String()},
	}
	body, err := json.Marshal(payload)
	if err != nil {
		sse.SendMessage(fmt.Sprintf("企业微信通知序列化失败: %v", err))
		return err
	}

	url := wecomWebhookBase + strings.TrimSpace(info.WebhookKey)
	client := &http.Client{Timeout: 10 * time.Second}
	resp, err := client.Post(url, "application/json", bytes.NewReader(body))
	if err != nil {
		sse.SendMessage(fmt.Sprintf("企业微信通知发送失败: %v", err))
		return err
	}
	defer resp.Body.Close()

	var resultResp struct {
		ErrCode int    `json:"errcode"`
		ErrMsg  string `json:"errmsg"`
	}
	if err := json.NewDecoder(resp.Body).Decode(&resultResp); err != nil {
		sse.SendMessage(fmt.Sprintf("企业微信通知响应解析失败: %v", err))
		return err
	}
	if resultResp.ErrCode != 0 {
		err := fmt.Errorf("企业微信返回错误: errcode=%d errmsg=%s", resultResp.ErrCode, resultResp.ErrMsg)
		sse.SendMessage(err.Error())
		return err
	}
	sse.SendMessage("企业微信通知已发送")
	return nil
}

func valueOrDash(s string) string {
	if strings.TrimSpace(s) == "" {
		return "-"
	}
	return s
}

// buildMentions 将逗号/分号/空格分隔的 userid 转换为企业微信 markdown 的 <@userid> 形式。
func buildMentions(raw string) string {
	fields := strings.FieldsFunc(raw, func(r rune) bool {
		switch r {
		case ',', '，', ';', '；', ' ', '\t', '\n', '\r':
			return true
		}
		return false
	})
	if len(fields) == 0 {
		return ""
	}
	mentions := make([]string, 0, len(fields))
	for _, f := range fields {
		if f = strings.TrimSpace(f); f != "" {
			mentions = append(mentions, fmt.Sprintf("<@%s>", f))
		}
	}
	return strings.Join(mentions, "")
}
