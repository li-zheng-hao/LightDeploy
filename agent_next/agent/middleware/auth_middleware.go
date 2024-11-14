package middleware

import (
	"net/http"
	"time"

	"lightdeploy.agentclient/config"
	"lightdeploy.agentclient/log"
	"lightdeploy.agentclient/utils"
)

// UseMiddleware 使用中间件
func UseMiddleware(handler http.HandlerFunc, middlewares ...func(http.HandlerFunc) http.HandlerFunc) http.HandlerFunc {
	// 首先添加 RecoveryMiddleware
	handler = RecoveryMiddleware(handler)

	// 添加其他中间件
	for _, middleware := range middlewares {
		handler = middleware(handler)
	}
	return handler
}

func AuthMiddleware(next http.HandlerFunc) http.HandlerFunc {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		// 如果配置了ApiKey，则进行ApiKey验证
		if config.GetConfig().Server.ApiKey != "" {
			apiKey := r.Header.Get("ApiKey")
			if apiKey != config.GetConfig().Server.ApiKey {
				http.Error(w, "Unauthorized", http.StatusUnauthorized)
				return
			} else {
				next.ServeHTTP(w, r)
				return
			}
		}
		// 如果未配置ApiKey，则不进行验证
		next.ServeHTTP(w, r)
	})
}

// 自定义 ResponseWriter 以捕获响应
type responseWriter struct {
	http.ResponseWriter
	body       []byte
	statusCode int
}

func (w *responseWriter) Write(b []byte) (int, error) {
	w.body = append(w.body, b...)
	return w.ResponseWriter.Write(b)
}

func (w *responseWriter) WriteHeader(statusCode int) {
	w.statusCode = statusCode
	w.ResponseWriter.WriteHeader(statusCode)
}

func LogRequestMiddleware(next http.HandlerFunc) http.HandlerFunc {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		startTime := time.Now()

		// 使用自定义的 ResponseWriter
		rw := &responseWriter{
			ResponseWriter: w,
			statusCode:     http.StatusOK, // 默认状态码为 200
		}
		next.ServeHTTP(rw, r)

		// 记录请求响应body和耗时
		log.Info("请求: [Method:%s] [Path:%s] 耗时: %s, 状态码: %d, 响应: %s",
			r.Method,
			r.URL.Path,
			time.Since(startTime),
			rw.statusCode,
			string(rw.body),
		)
	})
}

func RecoveryMiddleware(next http.HandlerFunc) http.HandlerFunc {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		defer func() {
			if err := recover(); err != nil {
				// 记录panic错误
				log.Error("服务发生异常: %v", err)
				// 返回500错误响应
				utils.WriteErrorJsonWithStatus(w, http.StatusInternalServerError, utils.ApiResponse{
					Code:    utils.API_ERROR_CODE,
					Message: "服务内部错误",
				})
			}
		}()
		next.ServeHTTP(w, r)
	})
}
