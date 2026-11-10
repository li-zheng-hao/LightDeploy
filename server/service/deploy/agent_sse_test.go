package deploy

import (
	"archive/zip"
	"bytes"
	"encoding/json"
	"fmt"
	"io"
	"ld_server/model"
	"ld_shared/dto"
	"ld_shared/sse"
	"net/http"
	"net/http/httptest"
	"os"
	"path/filepath"
	"strings"
	"sync/atomic"
	"testing"
	"time"
)

func TestCloseAgentSSEClosesHTTPStream(t *testing.T) {
	closed := make(chan struct{})
	release := make(chan struct{})
	server := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Content-Type", "text/event-stream")
		fmt.Fprint(w, "data: SSE连接已建立\n\n")
		w.(http.Flusher).Flush()
		select {
		case <-r.Context().Done():
			close(closed)
		case <-release:
		}
	}))
	defer server.Close()
	defer close(release)
	target := testDeployTarget(t, server, "")
	conn, err := connectToAgentSSE(target.Host, target.Port)
	if err != nil {
		t.Fatal(err)
	}
	closeAgentSSE(conn)
	select {
	case <-closed:
	case <-time.After(time.Second):
		t.Fatal("closing SSE connection left the HTTP stream open")
	}
	// 主流程结束与读取协程结束都可能调用关闭，必须可重复执行。
	closeAgentSSE(conn)
}

func TestFastDeployReportsUpdatedFilesToEverySubscriber(t *testing.T) {
	dir := t.TempDir()
	for _, name := range []string{"app.dll", "unchanged.json"} {
		if err := os.WriteFile(filepath.Join(dir, name), []byte("abc"), 0644); err != nil {
			t.Fatal(err)
		}
	}
	source, err := NewFastDeploySource(dir, "")
	if err != nil {
		t.Fatal(err)
	}
	first, unsubscribeFirst := sse.Subscribe()
	defer unsubscribeFirst()
	second, unsubscribeSecond := sse.Subscribe()
	defer unsubscribeSecond()
	var connections atomic.Int32
	release := make(chan struct{})
	server := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		switch r.URL.Path {
		case "/api/deploy/compare-files":
			var request dto.CompareFilesRequest
			if err := json.NewDecoder(r.Body).Decode(&request); err != nil {
				t.Error(err)
				w.WriteHeader(http.StatusBadRequest)
				return
			}
			var updates []dto.CompareFileInfo
			for _, info := range request.FileInfos {
				if info.FileRelativePath == "app.dll" {
					updates = append(updates, info)
				}
			}
			json.NewEncoder(w).Encode(dto.CompareFilesResponse{FileInfos: updates, SupportsContentHash: true})
		case "/api/sse/sse":
			connections.Add(1)
			w.Header().Set("Content-Type", "text/event-stream")
			fmt.Fprint(w, "data: SSE连接已建立\n\n")
			w.(http.Flusher).Flush()
			select {
			case <-r.Context().Done():
			case <-release:
			}
		case "/api/deploy/deploy-windows-service":
			file, _, err := r.FormFile("file")
			if err != nil {
				t.Error(err)
				w.WriteHeader(http.StatusBadRequest)
				return
			}
			defer file.Close()
			data, err := io.ReadAll(file)
			if err != nil {
				t.Error(err)
				return
			}
			archive, err := zip.NewReader(bytes.NewReader(data), int64(len(data)))
			if err != nil {
				t.Error(err)
				return
			}
			if len(archive.File) != 1 || archive.File[0].Name != "app.dll" {
				t.Errorf("deployment package did not match the reported update list: %+v", archive.File)
			}
			w.WriteHeader(http.StatusOK)
		default:
			t.Errorf("unexpected agent request: %s", r.URL.Path)
			w.WriteHeader(http.StatusNotFound)
		}
	}))
	defer server.Close()
	defer close(release)
	target := testDeployTarget(t, server, "target")
	if err := source.DeployToTarget(&target, &model.DeployService{OnlyCopyFile: true}); err != nil {
		t.Fatal(err)
	}
	if got := connections.Load(); got != 1 {
		t.Errorf("agent SSE connections = %d, want 1", got)
	}
	for i, messages := range []<-chan string{first, second} {
		foundFileList := false
		for {
			select {
			case message := <-messages:
				if strings.Contains(message, "需要更新 1 个文件: app.dll") {
					foundFileList = true
				}
				if message == "部署完成:"+target.Host {
					if !foundFileList {
						t.Fatalf("subscriber %d received completion without the updated file list", i+1)
					}
					goto nextSubscriber
				}
			case <-time.After(time.Second):
				t.Fatalf("subscriber %d did not receive deployment completion", i+1)
			}
		}
	nextSubscriber:
	}
}

func TestConnectToAgentSSERejectsHTTPError(t *testing.T) {
	server := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		w.WriteHeader(http.StatusInternalServerError)
	}))
	defer server.Close()
	target := testDeployTarget(t, server, "")
	conn, err := connectToAgentSSE(target.Host, target.Port)
	if conn != nil {
		closeAgentSSE(conn)
	}
	if err == nil {
		t.Fatal("SSE HTTP error treated as an established connection")
	}
}
