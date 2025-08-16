package middleware

import (
	"net/http"
	"runtime/debug"

	"ld_shared/clog"

	"github.com/gofiber/fiber/v2"
)

func ErrorMiddleware() fiber.Handler {
	return func(c *fiber.Ctx) error {
		defer func() {
			if err := recover(); err != nil {
				logger := clog.GetFiberContextLogger(c)
				// 打印堆栈信息
				logger.Error("未处理panic",
					"error", err,
					"stack", string(debug.Stack()),
					"path", c.Path(),
					"method", c.Method(),
				)
				c.Status(http.StatusInternalServerError).JSON(fiber.Map{
					"code": 500,
					"msg":  "服务器内部错误",
				})
			}
		}()

		return c.Next()
	}
}