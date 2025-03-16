package sse

import (
	"fmt"

	"ld_shared/sse"

	"github.com/gin-gonic/gin"
)

func HandleSSE(c *gin.Context) {
	// 设置 SSE 必要的 headers
	c.Header("Content-Type", "text/event-stream")

	// 创建一个通道用于关闭连接
	clientGone := c.Writer.CloseNotify()

	// 获取 Gin 的 ResponseWriter
	w := c.Writer

	for {
		select {
		case <-clientGone:
			// 客户端断开连接
			sse.ClearAllMessage()
			return
		case message := <-sse.MessageChan:
			// 发送消息
			fmt.Fprintf(w, "event: message\n")
			fmt.Fprintf(w, "data: %s\n\n", message)
			w.Flush()
		}
	}
}