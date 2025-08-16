package sse

import (
	"ld_shared/sse"

	"github.com/gofiber/fiber/v2"
)

func SendMessage(c *fiber.Ctx) error {
	message := c.Query("message")
	sse.MessageChan <- message
	return c.JSON(fiber.Map{
		"message": "message sent",
	})
}