package middleware

import (
	"ld_shared/clog"

	"github.com/gin-gonic/gin"
)

func LogMiddleware() gin.HandlerFunc {
	return func(c *gin.Context) {
		clog.SetContextLogger(c)
		c.Next() // 先调用c.Next()执行后面的中间件

	}
}
