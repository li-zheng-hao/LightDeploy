package sse

import (
	"bufio"
	"fmt"
	"log/slog"
	"strconv"
	"strings"
	"time"

	"ld_shared/sse"

	"github.com/gofiber/fiber/v2"
	"github.com/google/uuid"
	"github.com/valyala/fasthttp"
)

// 添加一个全局关闭信号通道
var (
	shutdownChan = make(chan struct{})
)

// 添加关闭所有SSE连接的函数
func ShutdownSSE() {
	close(shutdownChan)
}

func HandleSSE(c *fiber.Ctx) error {
	taskID := strings.Clone(c.Query("taskId"))
	if taskID != "" {
		if _, err := uuid.Parse(taskID); err != nil {
			return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{"message": "部署任务编号无效"})
		}
		return handleTaskSSE(c, taskID)
	}
	// 设置 SSE 必要的 headers
	c.Set("Content-Type", "text/event-stream")
	c.Set("Cache-Control", "no-cache")
	c.Set("Connection", "keep-alive")
	c.Set("Transfer-Encoding", "chunked")
	c.Set("X-Accel-Buffering", "no")

	// 获取连接的唯一标识符
	connID := fmt.Sprintf("%p", c.Context())
	slog.Info("新的SSE连接", "connectionID", connID)

	c.Context().SetBodyStreamWriter(fasthttp.StreamWriter(func(w *bufio.Writer) {
		defer slog.Info("SSE connection closed", "connectionID", connID)
		messages, unsubscribe := sse.Subscribe()
		defer unsubscribe()
		heartbeat := time.NewTicker(15 * time.Second)
		defer heartbeat.Stop()

		// 发送一个初始化消息
		fmt.Fprintf(w, "event: message\ndata: %s\n\n", "SSE连接已建立")
		if err := w.Flush(); err != nil {
			return
		}
		fmt.Fprint(w, "event: ready\ndata: ready\n\n")
		if err := w.Flush(); err != nil {
			return
		}

		for {
			select {
			case <-shutdownChan:
				slog.Info("服务器关闭，关闭所有SSE连接")
				return
			case <-heartbeat.C:
				fmt.Fprint(w, "event: ping\ndata: keep-alive\n\n")
				if err := w.Flush(); err != nil {
					return
				}
			case message, ok := <-messages:
				if !ok {
					return
				}
				// 发送消息
				slog.Info("发送SSE消息：", "message", message)
				fmt.Fprintf(w, "event: message\ndata: %s\n\n", message)
				err := w.Flush()
				if err != nil {
					slog.Error("SSE write error", "error", err, "connectionID", connID)
					return
				}
			}
		}
	}))
	return nil
}

func handleTaskSSE(c *fiber.Ctx, taskID string) error {
	cursor := c.Query("after", c.Get("Last-Event-ID", "0"))
	after, err := strconv.ParseUint(cursor, 10, 64)
	if err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{"message": "日志序号无效"})
	}
	history, messages, unsubscribe, err := sse.DeploymentLogs.Subscribe(taskID, after)
	if err != nil {
		// EventSource 不暴露 HTTP 状态码，使用终止事件避免过期任务无限重连。
		c.Set("Content-Type", "text/event-stream")
		c.Set("Cache-Control", "no-cache")
		return c.SendString("event: expired\ndata: " + err.Error() + "\n\n")
	}
	c.Set("Content-Type", "text/event-stream")
	c.Set("Cache-Control", "no-cache")
	c.Set("Connection", "keep-alive")
	c.Set("X-Accel-Buffering", "no")
	slog.Info("新的部署日志连接", "taskId", taskID, "after", after)
	c.Context().SetBodyStreamWriter(fasthttp.StreamWriter(func(w *bufio.Writer) {
		defer unsubscribe()
		defer slog.Info("部署日志连接关闭", "taskId", taskID)
		heartbeat := time.NewTicker(15 * time.Second)
		defer heartbeat.Stop()
		if history.Truncated {
			fmt.Fprint(w, "event: log-gap\ndata: truncated\n\n")
		}
		for _, event := range history.Events {
			writeLogEvent(w, event)
		}
		// 在历史回放及订阅完成后才通知前端可以提交部署。
		fmt.Fprint(w, "event: ready\ndata: ready\n\n")
		if err := w.Flush(); err != nil {
			return
		}
		for {
			select {
			case <-shutdownChan:
				return
			case <-heartbeat.C:
				fmt.Fprint(w, "event: ping\ndata: keep-alive\n\n")
				if err := w.Flush(); err != nil {
					return
				}
			case event, ok := <-messages:
				if !ok {
					latest, err := sse.DeploymentLogs.Snapshot(taskID, 0)
					if err == nil && latest.Finished {
						fmt.Fprint(w, "event: complete\ndata: complete\n\n")
						w.Flush()
					}
					return
				}
				writeLogEvent(w, event)
				if err := w.Flush(); err != nil {
					return
				}
			}
		}
	}))
	return nil
}

func writeLogEvent(w *bufio.Writer, event sse.LogEvent) {
	fmt.Fprintf(w, "id: %d\nevent: message\n", event.ID)
	for _, line := range strings.Split(strings.ReplaceAll(event.Message, "\r\n", "\n"), "\n") {
		fmt.Fprintf(w, "data: %s\n", line)
	}
	fmt.Fprint(w, "\n")
}
