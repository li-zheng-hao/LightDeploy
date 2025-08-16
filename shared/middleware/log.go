package middleware

import (
	"ld_shared/clog"

	"github.com/gofiber/fiber/v2"
)

func LogMiddleware() fiber.Handler {
	return func(c *fiber.Ctx) error {
		clog.SetFiberContextLogger(c)
		return c.Next()
	}
}