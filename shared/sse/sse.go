package sse

var (
	MessageChan = make(chan string, 100)
)

// 清空所有message
func ClearAllMessage() {
	for {
		select {
		case <-MessageChan:
			// 持续读取直到 channel 为空
		default:
			// channel 为空时返回
			return
		}
	}
}

func SendMessage(message string) {
	MessageChan <- message
}
