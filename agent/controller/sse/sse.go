package sse

import (
	"bufio"
	"fmt"
	"log/slog"

	"ld_shared/sse"

	"github.com/gofiber/fiber/v2"
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
	// 设置 SSE 必要的 headers
	c.Set("Content-Type", "text/event-stream")
	c.Set("Cache-Control", "no-cache")
	c.Set("Connection", "keep-alive")
	c.Set("Transfer-Encoding", "chunked")

	// 获取连接的唯一标识符
	connID := fmt.Sprintf("%p", c.Context())
	slog.Info("新的SSE连接", "connectionID", connID)

	c.Context().SetBodyStreamWriter(fasthttp.StreamWriter(func(w *bufio.Writer) {
		defer slog.Info("SSE connection closed", "connectionID", connID)

		// 发送一个初始化消息
		fmt.Fprintf(w, "event: message\ndata: %s\n\n", "SSE连接已建立")
		w.Flush()

		for {
			select {
			case <-shutdownChan:
				slog.Info("服务器关闭，关闭所有SSE连接")
				sse.ClearAllMessage()
				return
			case message := <-sse.MessageChan:
				// 发送消息
				slog.Info("发送SSE消息：", "message", message)
				fmt.Fprintf(w, "event: message\ndata: %s\n\n", message)
				err := w.Flush()
				if err != nil {
					slog.Error("SSE write error", "error", err, "connectionID", connID)
					sse.ClearAllMessage()
					return
				}
			}
		}
	}))
	return nil
}