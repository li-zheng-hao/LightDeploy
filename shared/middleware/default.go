package middleware

import (
	"github.com/gofiber/fiber/v2"
	"github.com/gofiber/fiber/v2/middleware/requestid"
)

func RegisterDefaultMiddleware(r *fiber.App) {
	r.Use(requestid.New())
	r.Use(LogMiddleware())
	r.Use(ErrorMiddleware())
}