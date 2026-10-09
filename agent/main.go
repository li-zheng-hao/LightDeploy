package main

import (
	"context"
	"flag"
	"fmt"
	"log/slog"
	"net/http"
	"os"
	"os/signal"
	"syscall"
	"time"

	"ld_agent/router"
	_ "ld_shared/clog"

	"github.com/gofiber/fiber/v2"
)

var (
	// 服务端口
	PORT = 9002
)

func main() {
	flag.IntVar(&PORT, "p", 9002, "服务端口")
	flag.Parse()

	slog.Info("服务启动", "port", PORT)

	// run 为平台相关逻辑：Windows 判断是否以服务模式运行，Linux 直接以交互/守护方式运行
	if err := run(); err != nil {
		slog.Error("服务运行失败", "error", err)
	}
}

// newApp 创建 fiber 应用
func newApp() *fiber.App {
	app := fiber.New(fiber.Config{
		BodyLimit: 5 * 1024 * 1024 * 1024, // 5GB 限制
	})
	router.RegisterRoutes(app)
	return app
}

// runInteractive 以交互模式运行（Windows 控制台 / Linux 前台或 systemd 托管）
func runInteractive() error {
	slog.Info("以交互模式运行")

	app := newApp()

	// 在单独的 goroutine 中启动服务器
	go func() {
		if err := app.Listen(fmt.Sprintf(":%d", PORT)); err != nil && err != http.ErrServerClosed {
			slog.Error("监听失败", "error", err)
		}
	}()

	// 等待中断信号
	quit := make(chan os.Signal, 1)
	signal.Notify(quit, syscall.SIGINT, syscall.SIGTERM)
	<-quit
	slog.Info("正在关闭服务器...")

	// 优雅地关闭服务器
	gracefulShutdown(app)
	return nil
}

func gracefulShutdown(app *fiber.App) {
	slog.Info("正在关闭服务器...")

	// 启动SSE关闭
	// sse.ShutdownSSE()
	// slog.Info("SSE连接已关闭")

	ctx, cancel := context.WithTimeout(context.Background(), 1*time.Second)
	defer cancel()
	if err := app.ShutdownWithContext(ctx); err != nil {
		slog.Error("服务器关闭失败", "error", err)
	}
	slog.Info("服务器已退出")
}