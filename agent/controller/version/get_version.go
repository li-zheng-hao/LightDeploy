package version

import (
	"github.com/gofiber/fiber/v2"
)

var (
	VERSION = "1.1.2"
)

func GetVersion(c *fiber.Ctx) error {
	return c.JSON(fiber.Map{
		"version": VERSION,
	})
}
