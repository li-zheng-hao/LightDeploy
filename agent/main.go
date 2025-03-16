package main

import (
	"context"
	"fmt"
	"log/slog"
	"net/http"
	"os"
	"os/signal"
	"syscall"
	"time"

	"ld_agent/controller"
	"ld_agent/internal/command"
	_ "ld_agent/internal/log"

	"github.com/gin-gonic/gin"
)

func main() {
	command.Cmd.Parse()

	slog.Info("启动服务", "port", command.Cmd.Port)
	
	if command.Cmd.Release {
		gin.SetMode(gin.ReleaseMode)
	}

	r := gin.Default()

	registerRoutes(r)

	srv := &http.Server{
		Addr:    fmt.Sprintf(":%d", command.Cmd.Port),
		Handler: r,
	}

	// 在单独的 goroutine 中启动服务器
	go func() {
		if err := srv.ListenAndServe(); err != nil && err != http.ErrServerClosed {
			slog.Error("监听失败", "error", err)
		}
	}()

	// 等待中断信号
	quit := make(chan os.Signal, 1)
	signal.Notify(quit, syscall.SIGINT, syscall.SIGTERM)
	<-quit
	slog.Info("正在关闭服务器...")

	// 设置超时时间为5秒的上下文
	ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancel()

	// 优雅地关闭服务器
	if err := srv.Shutdown(ctx); err != nil {
		slog.Error("服务器强制关闭", "error", err)
	}

	slog.Info("服务器已退出")
}

func registerRoutes(r *gin.Engine) {
	r.GET("/", func(c *gin.Context) {
		c.JSON(http.StatusOK, gin.H{
			"message": "pong",
		})
	})
	sseGroup := r.Group("/api/sse")
	sseGroup.GET("/sse", controller.HandleSSE)

	serviceGroup := r.Group("/api/service")
	serviceGroup.GET("/get-windows-service-status", controller.GetWindowsServiceStatus)

	deployGroup := r.Group("/api/deploy")
	deployGroup.POST("/deploy-windows-service", controller.DeployWindowsService)
}


