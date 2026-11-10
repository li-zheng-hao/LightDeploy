package sse

import (
	"fmt"
	"testing"
)

func TestSendMessageBroadcastsInOrder(t *testing.T) {
	first, unsubscribeFirst := Subscribe()
	defer unsubscribeFirst()
	second, unsubscribeSecond := Subscribe()
	defer unsubscribeSecond()
	messages := []string{"开始部署", "需要更新 1 个文件: app.dll", "部署完成"}
	for _, message := range messages {
		SendMessage(message)
	}
	for i, messagesChan := range []<-chan string{first, second} {
		for _, want := range messages {
			select {
			case got := <-messagesChan:
				if got != want {
					t.Fatalf("subscriber %d got %q, want %q", i+1, got, want)
				}
			default:
				t.Fatalf("subscriber %d missed %q", i+1, want)
			}
		}
	}
}

func TestUnsubscribeDoesNotClearOtherSubscribers(t *testing.T) {
	first, unsubscribeFirst := Subscribe()
	defer unsubscribeFirst()
	_, unsubscribeSecond := Subscribe()
	SendMessage("待发送的文件列表")
	unsubscribeSecond()
	unsubscribeSecond()
	SendMessage("部署完成")
	for _, want := range []string{"待发送的文件列表", "部署完成"} {
		select {
		case got := <-first:
			if got != want {
				t.Fatalf("got %q, want %q", got, want)
			}
		default:
			t.Fatalf("remaining subscriber missed %q", want)
		}
	}
}

func TestSlowSubscriberDoesNotBlockDeployment(t *testing.T) {
	slow, unsubscribeSlow := Subscribe()
	defer unsubscribeSlow()
	fast, unsubscribeFast := Subscribe()
	defer unsubscribeFast()
	for i := 0; i < 2000; i++ {
		want := fmt.Sprintf("日志 %d", i)
		SendMessage(want)
		select {
		case got := <-fast:
			if got != want {
				t.Fatalf("active subscriber got %q, want %q", got, want)
			}
		default:
			t.Fatal("slow subscriber blocked active subscriber")
		}
	}
	for range slow {
		// 缓冲耗尽后慢连接应已关闭，避免长期阻塞发布或泄漏内存。
	}
}

func TestSendMessageWithoutSubscribersDoesNotBlock(t *testing.T) {
	for i := 0; i < 2000; i++ {
		SendMessage("无页面连接时的日志")
	}
}
