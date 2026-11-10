package deploy

import "ld_shared/sse"

func deployLogger(senders []func(string)) func(string) {
	if len(senders) > 0 && senders[0] != nil {
		return senders[0]
	}
	return sse.SendMessage
}
