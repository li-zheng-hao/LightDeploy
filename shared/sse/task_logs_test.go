package sse

import (
	"testing"
	"time"
)

func TestTaskLogsReplayOnlyMissedMessagesForTheRequestedTask(t *testing.T) {
	logs := NewTaskLogs(10, 20, time.Minute)
	if err := logs.Start("first"); err != nil {
		t.Fatal(err)
	}
	logs.Send("first", "开始部署")
	logs.Send("first", "需要更新 1 个文件: app.dll")
	if err := logs.Start("second"); err != nil {
		t.Fatal(err)
	}
	logs.Send("second", "其他部署的日志")
	snapshot, live, unsubscribe, err := logs.Subscribe("first", 1)
	if err != nil {
		t.Fatal(err)
	}
	defer unsubscribe()
	if len(snapshot.Events) != 1 || snapshot.Events[0].ID != 2 || snapshot.Events[0].Message != "需要更新 1 个文件: app.dll" {
		t.Fatalf("missed file list not replayed: %+v", snapshot)
	}
	logs.Send("first", "部署完成")
	select {
	case event := <-live:
		if event.ID != 3 || event.Message != "部署完成" {
			t.Fatalf("live message out of order: %+v", event)
		}
	default:
		t.Fatal("message between replay and live subscription was lost")
	}
}

func TestTaskLogsRetainCompletedDeploymentAndRejectDuplicateSubmission(t *testing.T) {
	logs := NewTaskLogs(10, 20, time.Minute)
	_, _, unsubscribe, err := logs.Subscribe("task", 0)
	if err != nil {
		t.Fatal(err)
	}
	unsubscribe()
	if err := logs.Start("task"); err != nil {
		t.Fatal(err)
	}
	logs.Send("task", "需要更新 1 个文件: app.dll")
	logs.Finish("task")
	if err := logs.Start("task"); err == nil {
		t.Fatal("duplicate deployment task was allowed to run again")
	}
	snapshot, err := logs.Snapshot("task", 0)
	if err != nil || len(snapshot.Events) != 1 || snapshot.Events[0].Message != "需要更新 1 个文件: app.dll" {
		t.Fatalf("completed deployment logs unavailable: %+v, %v", snapshot, err)
	}
	if !snapshot.Finished {
		t.Fatal("snapshot did not identify the completed deployment")
	}
}

func TestFinishReleasesSubscribersAfterQueuedMessages(t *testing.T) {
	logs := NewTaskLogs(10, 20, time.Minute)
	_, messages, unsubscribe, err := logs.Subscribe("task", 0)
	if err != nil {
		t.Fatal(err)
	}
	defer unsubscribe()
	if err := logs.Start("task"); err != nil {
		t.Fatal(err)
	}
	logs.Send("task", "文件列表")
	logs.Finish("task")
	logs.Finish("task")
	if event := <-messages; event.Message != "文件列表" {
		t.Fatalf("finish discarded queued logs: %+v", event)
	}
	select {
	case _, ok := <-messages:
		if ok {
			t.Fatal("completed stream should be closed")
		}
	default:
		t.Fatal("completed deployment left its stream open")
	}
}

func TestTaskLogRetentionReportsMissingHistory(t *testing.T) {
	logs := NewTaskLogs(2, 2, time.Minute)
	if err := logs.Start("task"); err != nil {
		t.Fatal(err)
	}
	for _, message := range []string{"第一条", "第二条", "第三条"} {
		logs.Send("task", message)
	}
	snapshot, err := logs.Snapshot("task", 0)
	if err != nil || !snapshot.Truncated || len(snapshot.Events) != 2 || snapshot.Events[0].ID != 2 || snapshot.Events[1].ID != 3 {
		t.Fatalf("bounded history silently lost messages: %+v, %v", snapshot, err)
	}
	snapshot, err = logs.Snapshot("task", 2)
	if err != nil || snapshot.Truncated || len(snapshot.Events) != 1 || snapshot.Events[0].ID != 3 {
		t.Fatalf("cursor replay incorrect: %+v, %v", snapshot, err)
	}
}

func TestTaskLogCapacityDoesNotEvictRunningDeployments(t *testing.T) {
	logs := NewTaskLogs(1, 20, time.Minute)
	if err := logs.Start("running"); err != nil {
		t.Fatal(err)
	}
	logs.Send("running", "文件列表")
	if err := logs.Start("other"); err == nil {
		t.Fatal("capacity limit evicted a running deployment")
	}
	logs.Finish("running")
	if err := logs.Start("other"); err != nil {
		t.Fatal(err)
	}
	if _, err := logs.Snapshot("running", 0); err == nil {
		t.Fatal("old completed deployment was not evicted")
	}
	if _, _, _, err := logs.Subscribe("running", 1); err == nil {
		t.Fatal("expired replay cursor silently started an empty task")
	}
}
