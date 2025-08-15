package sse

import (
	"ld_shared/sse"

	"github.com/gin-gonic/gin"
)

func SendMessage(c *gin.Context) {
	message := c.Query("message")
	sse.MessageChan <- message
	c.JSON(200, gin.H{
		"message": "message sent",
	})
}
