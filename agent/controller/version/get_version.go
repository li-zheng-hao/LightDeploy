package version

import (
	"github.com/gofiber/fiber/v2"
)

var (
	VERSION = "1.3.1"
)

func GetVersion(c *fiber.Ctx) error {
	return c.JSON(fiber.Map{
		"version": VERSION,
	})
}
