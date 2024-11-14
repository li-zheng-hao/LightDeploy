package route

import (
	"net/http"

	"lightdeploy.agentclient/handler"
	"lightdeploy.agentclient/middleware"
)

func SetupRoutes() *http.ServeMux {
	mux := http.NewServeMux()

	// 部署相关路由
	mux.HandleFunc("/api/deploy/deploy", middleware.UseMiddleware(handler.DeployHandler, middleware.LogRequestMiddleware, middleware.AuthMiddleware))
	mux.HandleFunc("/api/deploy/comparedir", middleware.UseMiddleware(handler.CompareDirHandler, middleware.LogRequestMiddleware, middleware.AuthMiddleware))
	mux.HandleFunc("/api/deploy/compare", middleware.UseMiddleware(handler.CompareHandler, middleware.LogRequestMiddleware, middleware.AuthMiddleware))
	mux.HandleFunc("/api/deploy/ping", middleware.UseMiddleware(handler.PingHandler, middleware.LogRequestMiddleware, middleware.AuthMiddleware))
	mux.HandleFunc("/api/deploy/updateself", middleware.UseMiddleware(handler.UpdateSelfHandler, middleware.LogRequestMiddleware, middleware.AuthMiddleware))

	// Windows服务相关路由
	mux.HandleFunc("/api/deploy/checkserviceexist", middleware.UseMiddleware(handler.CheckServiceExistHandler, middleware.LogRequestMiddleware, middleware.AuthMiddleware))
	mux.HandleFunc("/api/deploy/installwindowsservice", middleware.UseMiddleware(handler.InstallWindowsServiceHandler, middleware.LogRequestMiddleware, middleware.AuthMiddleware))
	mux.HandleFunc("/api/deploy/startservice", middleware.UseMiddleware(handler.StartServiceHandler, middleware.LogRequestMiddleware, middleware.AuthMiddleware))
	mux.HandleFunc("/api/deploy/stopservice", middleware.UseMiddleware(handler.StopServiceHandler, middleware.LogRequestMiddleware, middleware.AuthMiddleware))
	mux.HandleFunc("/api/deploy/getstatus", middleware.UseMiddleware(handler.GetStatusHandler, middleware.LogRequestMiddleware, middleware.AuthMiddleware))

	// 其他功能路由
	mux.HandleFunc("/api/deploy/getagentversion", handler.GetAgentVersionHandler)
	mux.HandleFunc("/api/deploy/copy", middleware.UseMiddleware(handler.CopyFileHandler, middleware.LogRequestMiddleware, middleware.AuthMiddleware))

	return mux
}
