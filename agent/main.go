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

	"ld_agent/controller/sse"
	"ld_agent/router"
	_ "ld_shared/clog"
	"ld_shared/env"

	"github.com/gin-gonic/gin"
	"golang.org/x/sys/windows/svc"
)

var (
	// 服务端口
	PORT = 9002
)

func main() {
	flag.IntVar(&PORT, "p", 9002, "服务端口")
	flag.Parse()
	if !env.IsDebugMode() {
		gin.SetMode(gin.ReleaseMode)
	}
	slog.Info("服务启动", "port", PORT)
	// 检查是否以服务模式运行
	isService, err := svc.IsWindowsService()
	if err != nil {
		slog.Error("检查服务模式失败", "error", err)
		return
	}

	// 根据运行模式选择不同的启动方式
	if isService {
		err = runService()
	} else {
		err = runInteractive()
	}

	if err != nil {
		slog.Error("服务运行失败", "error", err)
	}
}

type agentService struct {
	srv *http.Server
}

func (s *agentService) Execute(args []string, r <-chan svc.ChangeRequest, changes chan<- svc.Status) (ssec bool, errno uint32) {
	const cmdsAccepted = svc.AcceptStop | svc.AcceptShutdown
	changes <- svc.Status{State: svc.StartPending}

	// 启动HTTP服务
	go func() {
		if err := s.srv.ListenAndServe(); err != nil && err != http.ErrServerClosed {
			slog.Error("监听失败", "error", err)
		}
	}()

	changes <- svc.Status{State: svc.Running, Accepts: cmdsAccepted}

	for {
		c := <-r
		switch c.Cmd {
		case svc.Interrogate:
			changes <- c.CurrentStatus
		case svc.Stop, svc.Shutdown:
			changes <- svc.Status{State: svc.StopPending}
			// 通知所有SSE连接关闭
			sse.ShutdownSSE()
			// 优雅关闭服务器
			ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
			defer cancel()
			if err := s.srv.Shutdown(ctx); err != nil {
				slog.Error("服务器强制关闭", "error", err)
			}
			slog.Info("服务器已退出")
			return
		}
	}
}

func runService() error {
	slog.Info("以Windows服务模式运行")

	r := gin.Default()
	router.RegisterRoutes(r)

	srv := &http.Server{
		Addr:    fmt.Sprintf(":%d", PORT),
		Handler: r,
	}

	return svc.Run("", &agentService{srv: srv})
}

func runInteractive() error {
	slog.Info("以交互模式运行")

	r := gin.Default()
	router.RegisterRoutes(r)

	srv := &http.Server{
		Addr:    fmt.Sprintf(":%d", PORT),
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
	return nil
}
