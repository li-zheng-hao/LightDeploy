package sse

import (
	"fmt"
	"log/slog"
	"sync"
	"time"
)

type LogEvent struct {
	ID      uint64 `json:"id"`
	Message string `json:"message"`
}

type LogSnapshot struct {
	Events    []LogEvent `json:"events"`
	Truncated bool       `json:"truncated"`
	Finished  bool       `json:"finished"`
}

type taskLog struct {
	events      []LogEvent
	lastID      uint64
	started     bool
	finished    bool
	updated     time.Time
	subscribers map[chan LogEvent]struct{}
}

// TaskLogs 保存有界的任务日志，回放与订阅在同一个锁内完成，避免两者之间丢消息。
type TaskLogs struct {
	mu        sync.Mutex
	tasks     map[string]*taskLog
	maxTasks  int
	maxEvents int
	retention time.Duration
}

// DeploymentLogs 保留最近一小时的部署日志；运行中的任务不会因过期被清理。
var DeploymentLogs = NewTaskLogs(100, 4096, time.Hour)

func NewTaskLogs(maxTasks, maxEvents int, retention time.Duration) *TaskLogs {
	return &TaskLogs{
		tasks: make(map[string]*taskLog), maxTasks: maxTasks,
		maxEvents: maxEvents, retention: retention,
	}
}

func (l *TaskLogs) task(id string, create bool) (*taskLog, error) {
	now := time.Now()
	for key, task := range l.tasks {
		if (!task.started || task.finished) && now.Sub(task.updated) > l.retention {
			l.remove(key)
		}
	}
	if task, ok := l.tasks[id]; ok {
		return task, nil
	}
	if !create {
		return nil, fmt.Errorf("部署日志已过期或任务不存在，请重新部署")
	}
	if len(l.tasks) >= l.maxTasks {
		var oldestID string
		var oldestTime time.Time
		for key, task := range l.tasks {
			if task.started && !task.finished {
				continue
			}
			if oldestID == "" || task.updated.Before(oldestTime) {
				oldestID, oldestTime = key, task.updated
			}
		}
		if oldestID == "" {
			return nil, fmt.Errorf("同时部署的任务过多，请稍后重试")
		}
		l.remove(oldestID)
	}
	task := &taskLog{updated: now, subscribers: make(map[chan LogEvent]struct{})}
	l.tasks[id] = task
	return task, nil
}

func (l *TaskLogs) remove(id string) {
	for subscriber := range l.tasks[id].subscribers {
		close(subscriber)
	}
	delete(l.tasks, id)
}

func (l *TaskLogs) Start(id string) error {
	l.mu.Lock()
	defer l.mu.Unlock()
	task, err := l.task(id, true)
	if err != nil {
		return err
	}
	if task.started {
		return fmt.Errorf("部署任务已提交，请勿重复部署")
	}
	task.started = true
	task.updated = time.Now()
	return nil
}

func (l *TaskLogs) Finish(id string) {
	l.mu.Lock()
	defer l.mu.Unlock()
	if task := l.tasks[id]; task != nil {
		task.finished = true
		task.updated = time.Now()
		for subscriber := range task.subscribers {
			delete(task.subscribers, subscriber)
			close(subscriber)
		}
	}
}

func (l *TaskLogs) Send(id, message string) {
	l.mu.Lock()
	defer l.mu.Unlock()
	task := l.tasks[id]
	if task == nil {
		return
	}
	task.lastID++
	event := LogEvent{ID: task.lastID, Message: message}
	if len(task.events) == l.maxEvents {
		copy(task.events, task.events[1:])
		task.events[len(task.events)-1] = event
	} else {
		task.events = append(task.events, event)
	}
	task.updated = time.Now()
	for subscriber := range task.subscribers {
		select {
		case subscriber <- event:
		default:
			delete(task.subscribers, subscriber)
			close(subscriber)
		}
	}
}

func snapshot(task *taskLog, after uint64) LogSnapshot {
	result := LogSnapshot{Events: make([]LogEvent, 0), Finished: task.finished}
	if len(task.events) > 0 {
		result.Truncated = after < task.events[0].ID-1
	}
	for _, event := range task.events {
		if event.ID > after {
			result.Events = append(result.Events, event)
		}
	}
	return result
}

func (l *TaskLogs) Snapshot(id string, after uint64) (LogSnapshot, error) {
	l.mu.Lock()
	defer l.mu.Unlock()
	task, err := l.task(id, false)
	if err != nil {
		return LogSnapshot{}, err
	}
	return snapshot(task, after), nil
}

func (l *TaskLogs) Subscribe(id string, after uint64) (LogSnapshot, <-chan LogEvent, func(), error) {
	l.mu.Lock()
	defer l.mu.Unlock()
	task, err := l.task(id, after == 0)
	if err != nil {
		return LogSnapshot{}, nil, nil, err
	}
	messages := make(chan LogEvent, 1024)
	if task.finished {
		close(messages)
	} else {
		task.subscribers[messages] = struct{}{}
	}
	unsubscribe := func() {
		l.mu.Lock()
		defer l.mu.Unlock()
		if current := l.tasks[id]; current == task {
			if _, ok := task.subscribers[messages]; ok {
				delete(task.subscribers, messages)
				close(messages)
			}
		}
	}
	return snapshot(task, after), messages, unsubscribe, nil
}

// TaskSender 同时保留任务日志和原有广播，兼容旧页面及其他服务操作。
func TaskSender(id string) func(string) {
	return func(message string) {
		slog.Info("部署日志", "taskId", id, "message", message)
		DeploymentLogs.Send(id, message)
		SendMessage(message)
	}
}
