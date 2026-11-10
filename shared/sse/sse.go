package sse

import (
	"log/slog"
	"sync"
)

var (
	subscribersMu sync.Mutex
	subscribers   = make(map[chan string]struct{})
)

// Subscribe 为每个 SSE 连接创建独立队列，避免多个连接争抢日志。
func Subscribe() (<-chan string, func()) {
	messages := make(chan string, 1024)
	subscribersMu.Lock()
	subscribers[messages] = struct{}{}
	subscribersMu.Unlock()

	return messages, func() {
		subscribersMu.Lock()
		defer subscribersMu.Unlock()
		if _, ok := subscribers[messages]; ok {
			delete(subscribers, messages)
			close(messages)
		}
	}
}

// SendMessage 向所有连接广播，未连接或已停止读取的客户端不阻塞部署。
func SendMessage(message string) {
	subscribersMu.Lock()
	defer subscribersMu.Unlock()
	for messages := range subscribers {
		select {
		case messages <- message:
		default:
			delete(subscribers, messages)
			close(messages)
			slog.Warn("SSE 客户端读取过慢，关闭该订阅")
		}
	}
}
