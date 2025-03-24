package middleware

import (
	"fmt"
	"net/http"
	"runtime/debug"

	"ld_shared/clog"
	"ld_shared/error_response"

	"github.com/gin-gonic/gin"
)

func ErrorMiddleware() gin.HandlerFunc {
	return func(c *gin.Context) {
		logger := clog.GetContextLogger(c)
		defer func() {
			if err := recover(); err != nil {
				// 打印堆栈信息
				logger.Error("未处理panic",
					"error", err,
					"stack", string(debug.Stack()),
					"path", c.Request.URL.Path,
					"method", c.Request.Method,
				)
				c.JSON(http.StatusOK, gin.H{
					"code": 500,
					"msg":  "服务器内部错误",
				})
			}
		}()

		c.Next() // 先调用c.Next()执行后面的中间件
		// 检查c.Errors中是否有错误
		for _, e := range c.Errors {
			err := e.Err
			// 若是自定义的错误则将code、msg返回
			if myErr, ok := err.(*error_response.ErrorResponse); ok {
				c.JSON(http.StatusOK, gin.H{
					"code": myErr.Code,
					"msg":  myErr.Message,
				})
			} else {
				// 若非自定义错误则返回详细错误信息err.Error()
				logStr := fmt.Sprintf("%+v\n", err)
				logger.Error("未处理错误", "error", logStr)
				// 比如save session出错时设置的err
				c.JSON(http.StatusOK, gin.H{
					"code": 500,
					"msg":  "系统异常",
				})
			}
			return
		}
	}
}
