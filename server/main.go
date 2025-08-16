package main

import (
	"context"
	"io"
	"log/slog"
	"net/http"
	"os"
	"os/signal"
	"strings"
	"syscall"
	"time"

	"ld_server/controller/sse"
	"ld_server/db"
	"ld_server/router"
	"ld_server/static"
	_ "ld_shared/clog"
	"ld_shared/middleware"

	"github.com/gofiber/fiber/v2"
	"golang.org/x/sys/windows/svc"
)

func initServer() *fiber.App {

	app := fiber.New(fiber.Config{
		BodyLimit: 5 * 1024 * 1024 * 1024, // 5GB 限制
	})
	middleware.RegisterDefaultMiddleware(app)
	registerRoutes(app)

	slog.Info("服务器将在以下地址启动", "address", ":31003")
	return app
}

func gracefulShutdown(app *fiber.App) {
	slog.Info("正在关闭服务器...")

	sse.ShutdownSSE()
	slog.Info("SSE连接已关闭")

	// 设置5秒超时关闭
	ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancel()
	if err := app.ShutdownWithContext(ctx); err != nil {
		slog.Error("服务器关闭失败", "error", err)
	}
	slog.Info("服务器已退出")
}

func main() {
	// 确保日志已经初始化
	slog.Info("开始初始化服务...")

	if err := db.Init(); err != nil {
		slog.Error("数据库初始化失败", "error", err)
		return
	}
	slog.Info("数据库初始化成功")

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
	app *fiber.App
}

func (s *serverService) Execute(args []string, r <-chan svc.ChangeRequest, changes chan<- svc.Status) (ssec bool, errno uint32) {
	const cmdsAccepted = svc.AcceptStop | svc.AcceptShutdown
	changes <- svc.Status{State: svc.StartPending}

	// 启动HTTP服务
	go func() {
		if err := s.app.Listen(":31003"); err != nil && err != http.ErrServerClosed {
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
			gracefulShutdown(s.app)
			return
		}
	}
}

func runService() error {
	slog.Info("以Windows服务模式运行")
	app := initServer()
	return svc.Run("", &serverService{app: app})
}

func runInteractive() error {
	slog.Info("以交互模式运行")
	app := initServer()

	// 在单独的 goroutine 中启动服务器
	go func() {
		if err := app.Listen(":31003"); err != nil && err != http.ErrServerClosed {
			slog.Error("监听失败", "error", err)
		}
	}()

	// 等待中断信号
	quit := make(chan os.Signal, 1)
	signal.Notify(quit, syscall.SIGINT, syscall.SIGTERM)
	<-quit

	gracefulShutdown(app)
	return nil
}

func registerRoutes(app *fiber.App) {
	// 注册嵌入的静态文件服务
	app.Get("/ui/*", func(c *fiber.Ctx) error {
		// 获取请求的文件路径
		filePath := c.Params("*")
		if filePath == "" {
			filePath = "index.html"
		}

		// 从嵌入的文件系统中读取文件
		file, err := static.GetDistFS().Open(filePath)
		if err != nil {
			// 如果文件不存在，返回 index.html（SPA fallback）
			indexContent, err := static.GetDistFS().Open("index.html")
			if err != nil {
				return c.Status(404).SendString("Not Found")
			}
			defer indexContent.Close()

			content, err := io.ReadAll(indexContent)
			if err != nil {
				return c.Status(500).SendString("Internal Server Error")
			}

			c.Set("Content-Type", "text/html")
			return c.Send(content)
		}
		defer file.Close()

		// 动态设置正确的 Content-Type 基于文件扩展名
		if c.Path() == "/ui/index.html" || c.Path() == "/ui" {
			c.Set("Content-Type", "text/html")
		} else {
			// 根据文件扩展名设置 Content-Type
			switch {
			case c.Path() == "/ui" || c.Path() == "/ui/":
				c.Set("Content-Type", "text/html")
			case c.Path() == "/ui/index.html":
				c.Set("Content-Type", "text/html")
			default:
				// 从文件路径中提取扩展名
				if len(filePath) > 0 {
					switch {
					case strings.HasSuffix(filePath, ".js"):
						c.Set("Content-Type", "application/javascript")
					case strings.HasSuffix(filePath, ".css"):
						c.Set("Content-Type", "text/css")
					case strings.HasSuffix(filePath, ".png"):
						c.Set("Content-Type", "image/png")
					case strings.HasSuffix(filePath, ".jpg"), strings.HasSuffix(filePath, ".jpeg"):
						c.Set("Content-Type", "image/jpeg")
					case strings.HasSuffix(filePath, ".gif"):
						c.Set("Content-Type", "image/gif")
					case strings.HasSuffix(filePath, ".svg"):
						c.Set("Content-Type", "image/svg+xml")
					case strings.HasSuffix(filePath, ".ico"):
						c.Set("Content-Type", "image/x-icon")
					case strings.HasSuffix(filePath, ".woff"):
						c.Set("Content-Type", "font/woff")
					case strings.HasSuffix(filePath, ".woff2"):
						c.Set("Content-Type", "font/woff2")
					case strings.HasSuffix(filePath, ".ttf"):
						c.Set("Content-Type", "font/ttf")
					case strings.HasSuffix(filePath, ".eot"):
						c.Set("Content-Type", "application/vnd.ms-fontobject")
					case strings.HasSuffix(filePath, ".json"):
						c.Set("Content-Type", "application/json")
					case strings.HasSuffix(filePath, ".xml"):
						c.Set("Content-Type", "application/xml")
					case strings.HasSuffix(filePath, ".txt"):
						c.Set("Content-Type", "text/plain")
					default:
						c.Set("Content-Type", "application/octet-stream")
					}
				}
			}
		}

		// 读取并返回文件内容
		content, err := io.ReadAll(file)
		if err != nil {
			return c.Status(500).SendString("Internal Server Error")
		}

		return c.Send(content)
	})

	// 注册API路由
	router.RegisterRoutes(app)
}
