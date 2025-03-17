package sse

import (
	"fmt"
	"log/slog"

	"ld_shared/sse"

	"github.com/gin-gonic/gin"
)

// 添加一个全局关闭信号通道
var (
	shutdownChan = make(chan struct{})
)

// 添加关闭所有SSE连接的函数
func ShutdownSSE() {
	close(shutdownChan)
}
func HandleSSE(c *gin.Context) {
	// 设置 SSE 必要的 headers
	c.Header("Content-Type", "text/event-stream")
	c.Header("Cache-Control", "no-cache")
	c.Header("Connection", "keep-alive")
	// 创建一个通道用于关闭连接
	clientGone := c.Writer.CloseNotify()

	// 获取 Gin 的 ResponseWriter
	w := c.Writer

	// 发送一个初始化消息
	fmt.Fprintf(w, "event: message\n")
	fmt.Fprintf(w, "data: %s\n\n", "SSE连接已建立")
	w.Flush()
	// 获取连接的唯一标识符
	connID := fmt.Sprintf("%p", c.Writer)
	slog.Info("新的SSE连接", "connectionID", connID)
	for {
		select {
		case <-clientGone:
			slog.Info("SSE连接关闭", "connectionID", connID)
			// 客户端断开连接
			sse.ClearAllMessage()
			return
		case <-shutdownChan:
			slog.Info("服务器关闭，关闭所有SSE连接")
			sse.ClearAllMessage()
			return
		case message := <-sse.MessageChan:
			// 发送消息
			slog.Info("发送SSE消息：", "message", message)
			fmt.Fprintf(w, "event: message\n")
			fmt.Fprintf(w, "data: %s\n\n", message)
			w.Flush()
		}
	}
}
