package sse

import (
	"bufio"
	"context"
	sharedSSE "ld_shared/sse"
	"net"
	"net/http"
	"strings"
	"testing"
	"time"

	"github.com/gofiber/fiber/v2"
)

func TestHandleSSEBroadcastsDeploymentLogs(t *testing.T) {
	app := fiber.New(fiber.Config{DisableStartupMessage: true})
	app.Get("/events", HandleSSE)
	listener, err := net.Listen("tcp", "127.0.0.1:0")
	if err != nil {
		t.Fatal(err)
	}
	oldShutdown := shutdownChan
	shutdownChan = make(chan struct{})
	stopped := make(chan error, 1)
	go func() { stopped <- app.Listener(listener) }()
	t.Cleanup(func() {
		ShutdownSSE()
		if err := app.ShutdownWithTimeout(2 * time.Second); err != nil {
			t.Error(err)
		}
		if err := <-stopped; err != nil {
			t.Error(err)
		}
		shutdownChan = oldShutdown
	})

	readers := make([]*bufio.Reader, 2)
	for i := range readers {
		ctx, cancel := context.WithTimeout(context.Background(), 3*time.Second)
		t.Cleanup(cancel)
		req, err := http.NewRequestWithContext(ctx, http.MethodGet, "http://"+listener.Addr().String()+"/events", nil)
		if err != nil {
			t.Fatal(err)
		}
		response, err := http.DefaultClient.Do(req)
		if err != nil {
			t.Fatal(err)
		}
		t.Cleanup(func() { response.Body.Close() })
		readers[i] = bufio.NewReader(response.Body)
		if got := readSSEMessage(t, readers[i]); got != "SSE连接已建立" {
			t.Fatalf("initial message = %q", got)
		}
	}

	messages := []string{
		"开始快速模式部署",
		"目标机器 10.12.49.19 需要更新 1 个文件: app.dll",
		"部署完成:10.12.49.19",
	}
	for _, message := range messages {
		sharedSSE.SendMessage(message)
	}
	for i, reader := range readers {
		for _, want := range messages {
			if got := readSSEMessage(t, reader); got != want {
				t.Fatalf("client %d got %q, want %q", i+1, got, want)
			}
		}
	}
}

func readSSEMessage(t *testing.T, reader *bufio.Reader) string {
	t.Helper()
	for {
		line, err := reader.ReadString('\n')
		if err != nil {
			t.Fatalf("read SSE message: %v", err)
		}
		if strings.HasPrefix(line, "data: ") {
			return strings.TrimSuffix(strings.TrimPrefix(line, "data: "), "\n")
		}
	}
}
