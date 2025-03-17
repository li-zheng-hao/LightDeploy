package clog

import (
	"context"
	"log/slog"
	"os"
	"path/filepath"

	"gopkg.in/natefinch/lumberjack.v2"
)

// MultiHandler 实现多个handler的输出
type MultiHandler struct {
	handlers []slog.Handler
}

func (h *MultiHandler) Enabled(ctx context.Context, level slog.Level) bool {
	// 如果任一handler启用了该级别，则启用
	for _, handler := range h.handlers {
		if handler.Enabled(ctx, level) {
			return true
		}
	}
	return false
}

func (h *MultiHandler) Handle(ctx context.Context, r slog.Record) error {
	for _, handler := range h.handlers {
		if err := handler.Handle(ctx, r); err != nil {
			return err
		}
	}
	return nil
}

func (h *MultiHandler) WithAttrs(attrs []slog.Attr) slog.Handler {
	var handlers []slog.Handler
	for _, handler := range h.handlers {
		handlers = append(handlers, handler.WithAttrs(attrs))
	}
	return &MultiHandler{handlers: handlers}
}

func (h *MultiHandler) WithGroup(name string) slog.Handler {
	var handlers []slog.Handler
	for _, handler := range h.handlers {
		handlers = append(handlers, handler.WithGroup(name))
	}
	return &MultiHandler{handlers: handlers}
}

func NewMultiHandler(handlers ...slog.Handler) *MultiHandler {
	return &MultiHandler{handlers: handlers}
}

func init() {
	// 获取可执行文件路径
	execPath, err := os.Executable()
	if err != nil {
		panic(err)
	}

	// 设置日志文件路径
	logDir := filepath.Join(filepath.Dir(execPath), "logs")
	if err := os.MkdirAll(logDir, 0755); err != nil {
		panic(err)
	}
	logPath := filepath.Join(logDir, "app.log")

	// 创建JSON格式的文件处理器，带有日志轮转功能
	fileHandler := slog.NewJSONHandler(&lumberjack.Logger{
		Filename:   logPath, // 日志文件路径
		MaxSize:    100,     // 每个日志文件最大100MB
		MaxBackups: 7,       // 保留7个备份
		MaxAge:     7,       // 保留7天
		Compress:   true,    // 压缩旧的日志文件
	}, &slog.HandlerOptions{
		Level: slog.LevelDebug,
	})

	// 创建控制台处理器
	consoleHandler := slog.NewTextHandler(os.Stdout, &slog.HandlerOptions{
		Level: slog.LevelDebug,
	})

	// 创建多处理器
	handler := NewMultiHandler(fileHandler, consoleHandler)

	// 设置全局logger
	logger := slog.New(handler)
	slog.SetDefault(logger)
}
