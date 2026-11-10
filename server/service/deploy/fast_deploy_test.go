package deploy

import (
	"encoding/json"
	"ld_server/model"
	"ld_shared/dto"
	"net"
	"net/http"
	"net/http/httptest"
	"os"
	"path/filepath"
	"strconv"
	"testing"
	"time"
)

func TestFastDeploySourceHashesOnlyRequestedFiles(t *testing.T) {
	dir := t.TempDir()
	for _, name := range []string{"same.dll", "changed.dll", "ignored.txt"} {
		if err := os.WriteFile(filepath.Join(dir, name), []byte("abc"), 0644); err != nil {
			t.Fatal(err)
		}
	}
	source, err := NewFastDeploySource(dir, `ignored\.txt$`)
	if err != nil {
		t.Fatal(err)
	}
	requests := 0
	server := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		requests++
		var request dto.CompareFilesRequest
		if err := json.NewDecoder(r.Body).Decode(&request); err != nil {
			t.Error(err)
			w.WriteHeader(http.StatusBadRequest)
			return
		}
		if !request.UseContentHash || request.ServicePath != "target-dir" {
			t.Errorf("unexpected request: %+v", request)
		}
		result := dto.CompareFilesResponse{SupportsContentHash: true}
		if requests%2 == 1 {
			if len(request.FileInfos) != 2 {
				t.Errorf("metadata file count = %d, want 2", len(request.FileInfos))
			}
			for _, file := range request.FileInfos {
				if file.ModifyTimeNano == 0 {
					t.Error("metadata round omitted nanosecond modification time")
				}
				if file.ContentHash != "" {
					t.Error("metadata round included a content hash")
				}
				if file.FileRelativePath == "same.dll" {
					result.HashRequiredFileInfos = append(result.HashRequiredFileInfos, file)
				} else {
					result.FileInfos = append(result.FileInfos, file)
				}
			}
		} else {
			if len(request.FileInfos) != 1 || request.FileInfos[0].FileRelativePath != "same.dll" {
				t.Errorf("hash round included unrequested files: %+v", request.FileInfos)
			} else if request.FileInfos[0].ContentHash != "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad" {
				t.Errorf("unexpected content hash: %s", request.FileInfos[0].ContentHash)
			}
		}
		if err := json.NewEncoder(w).Encode(result); err != nil {
			t.Error(err)
		}
	}))
	defer server.Close()
	target := testDeployTarget(t, server, "target-dir")
	for i := 0; i < 2; i++ {
		result, err := source.CompareWithTarget(target)
		if err != nil {
			t.Fatal(err)
		}
		if len(result.FileInfos) != 1 || result.FileInfos[0].FileRelativePath != "changed.dll" {
			t.Fatalf("unexpected update list: %+v", result.FileInfos)
		}
	}
	if requests != 4 {
		t.Fatalf("requests = %d, want 4", requests)
	}
}

func TestFastDeploySourceRejectsChangesAfterScan(t *testing.T) {
	dir := t.TempDir()
	path := filepath.Join(dir, "app.dll")
	if err := os.WriteFile(path, []byte("abc"), 0644); err != nil {
		t.Fatal(err)
	}
	source, err := NewFastDeploySource(dir, "")
	if err != nil {
		t.Fatal(err)
	}
	info, err := os.Stat(path)
	if err != nil {
		t.Fatal(err)
	}
	if err := os.WriteFile(path, []byte("xyz"), 0644); err != nil {
		t.Fatal(err)
	}
	changedTime := info.ModTime().Add(time.Millisecond)
	if err := os.Chtimes(path, changedTime, changedTime); err != nil {
		t.Fatal(err)
	}
	server := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		var request dto.CompareFilesRequest
		if err := json.NewDecoder(r.Body).Decode(&request); err != nil {
			t.Error(err)
			return
		}
		json.NewEncoder(w).Encode(dto.CompareFilesResponse{
			SupportsContentHash: true, HashRequiredFileInfos: request.FileInfos,
		})
	}))
	defer server.Close()
	if _, err := source.CompareWithTarget(testDeployTarget(t, server, dir)); err == nil {
		t.Fatal("source changed after scan without an error")
	}
}

func TestFastDeploySourceSupportsLegacyAgent(t *testing.T) {
	dir := t.TempDir()
	if err := os.WriteFile(filepath.Join(dir, "app.dll"), []byte("abc"), 0644); err != nil {
		t.Fatal(err)
	}
	source, err := NewFastDeploySource(dir, "")
	if err != nil {
		t.Fatal(err)
	}
	requests := 0
	server := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		requests++
		json.NewEncoder(w).Encode(map[string]any{"fileInfos": source.fileInfos})
	}))
	defer server.Close()
	result, err := source.CompareWithTarget(testDeployTarget(t, server, dir))
	if err != nil {
		t.Fatal(err)
	}
	if requests != 1 || len(result.FileInfos) != 1 {
		t.Fatalf("legacy comparison: requests=%d, updates=%d", requests, len(result.FileInfos))
	}
}

func TestFastDeploySourceEmptyDirectoryNeedsNoRequest(t *testing.T) {
	source, err := NewFastDeploySource(t.TempDir(), "")
	if err != nil {
		t.Fatal(err)
	}
	result, err := source.CompareWithTarget(model.DeployTarget{})
	if err != nil || len(result.FileInfos) != 0 {
		t.Fatalf("empty source: result=%+v, error=%v", result, err)
	}
}

func testDeployTarget(t *testing.T, server *httptest.Server, servicePath string) model.DeployTarget {
	t.Helper()
	host, port, err := net.SplitHostPort(server.Listener.Addr().String())
	if err != nil {
		t.Fatal(err)
	}
	portNumber, err := strconv.Atoi(port)
	if err != nil {
		t.Fatal(err)
	}
	return model.DeployTarget{Host: host, Port: portNumber, ServicePath: servicePath}
}
