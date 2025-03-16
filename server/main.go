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

	"ld_server/controller/sse"
	"ld_server/db"
	"ld_server/router"
	"ld_server/static"
	_ "ld_shared/log"

	"github.com/gin-gonic/gin"
	"golang.org/x/sys/windows/svc"
)

func main() {
	// 确保日志已经初始化
	slog.Info("开始初始化服务...")

	if err := db.Init(); err != nil {
		slog.Error("数据库初始化失败", "error", err)
		return
	}
	slog.Info("数据库初始化成功")

	gin.SetMode(gin.ReleaseMode)

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

type serverService struct {
	srv *http.Server
}

func (s *serverService) Execute(args []string, r <-chan svc.ChangeRequest, changes chan<- svc.Status) (ssec bool, errno uint32) {
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
	registerRoutes(r)

	srv := &http.Server{
		Addr:    fmt.Sprintf("localhost:%d", 31003),
		Handler: r,
	}
	slog.Info("服务器将在以下地址启动", "address", srv.Addr)

	return svc.Run("", &serverService{srv: srv})
}

func runInteractive() error {
	slog.Info("以交互模式运行")

	r := gin.Default()
	registerRoutes(r)

	srv := &http.Server{
		Addr:    fmt.Sprintf("localhost:%d", 31003),
		Handler: r,
	}
	slog.Info("服务器将在以下地址启动", "address", srv.Addr)

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
	// 通知所有SSE连接关闭
	sse.ShutdownSSE()
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

func registerRoutes(r *gin.Engine) {
	// 注册静态文件服务
	r.StaticFS("/ui", static.GetDistFS())

	// 注册API路由
	router.RegisterRoutes(r)
}


