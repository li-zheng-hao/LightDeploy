//go:build windows

package main

import (
	"context"
	"fmt"
	"log/slog"
	"net/http"
	"time"

	"ld_agent/controller/sse"

	"github.com/gofiber/fiber/v2"
	"golang.org/x/sys/windows/svc"
)

// run Windows 平台启动逻辑：判断是否以 Windows 服务模式运行
func run() error {
	isService, err := svc.IsWindowsService()
	if err != nil {
		return fmt.Errorf("检查服务模式失败: %v", err)
	}
	if isService {
		return runService()
	}
	return runInteractive()
}

type agentService struct {
	app *fiber.App
}

func (s *agentService) Execute(args []string, r <-chan svc.ChangeRequest, changes chan<- svc.Status) (ssec bool, errno uint32) {
	const cmdsAccepted = svc.AcceptStop | svc.AcceptShutdown
	changes <- svc.Status{State: svc.StartPending}

	// 启动HTTP服务
	go func() {
		if err := s.app.Listen(fmt.Sprintf(":%d", PORT)); err != nil && err != http.ErrServerClosed {
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
			if err := s.app.ShutdownWithContext(ctx); err != nil {
				slog.Error("服务器强制关闭", "error", err)
			}
			slog.Info("服务器已退出")
			return
		}
	}
}

func runService() error {
	slog.Info("以Windows服务模式运行")
	return svc.Run("", &agentService{app: newApp()})
}