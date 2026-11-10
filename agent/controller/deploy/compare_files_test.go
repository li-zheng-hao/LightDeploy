package deploy

import (
	"bytes"
	"encoding/json"
	"errors"
	"ld_shared/dto"
	"net/http"
	"net/http/httptest"
	"os"
	"path/filepath"
	"testing"
	"time"

	"github.com/gofiber/fiber/v2"
)

func TestCompareFilesContentCheck(t *testing.T) {
	const abcHash = "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad"
	tests := []struct {
		name        string
		content     string
		missing     bool
		archiveTime bool
		timeOffset  time.Duration
		sourceSize  int64
		hash        string
		wantUpdates int
		wantHash    int
	}{
		{name: "same content with changed timestamp", content: "abc", timeOffset: time.Minute, sourceSize: 3, hash: abcHash},
		{name: "different content with same size", content: "xyz", timeOffset: time.Minute, sourceSize: 3, hash: abcHash, wantUpdates: 1},
		{name: "provided hash takes precedence over timestamp", content: "xyz", sourceSize: 3, hash: abcHash, wantUpdates: 1},
		{name: "unchanged metadata needs no hash", content: "abc", sourceSize: 3},
		{name: "archive timestamp precision needs no hash", content: "abc", archiveTime: true, sourceSize: 3},
		{name: "changed timestamp requests hash", content: "abc", timeOffset: time.Minute, sourceSize: 3, wantHash: 1},
		{name: "small timestamp change requests hash", content: "abc", timeOffset: time.Second, sourceSize: 3, wantHash: 1},
		{name: "missing file needs no hash", missing: true, sourceSize: 3, wantUpdates: 1},
		{name: "different size needs no hash", content: "longer", sourceSize: 3, wantUpdates: 1},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			dir := t.TempDir()
			sourceTime := time.Unix(1700000000, 456000000)
			if !tt.missing {
				path := filepath.Join(dir, "app.dll")
				if err := os.WriteFile(path, []byte(tt.content), 0644); err != nil {
					t.Fatal(err)
				}
				targetTime := sourceTime.Add(tt.timeOffset)
				if tt.archiveTime {
					targetTime = targetTime.Truncate(time.Second)
				}
				if err := os.Chtimes(path, targetTime, targetTime); err != nil {
					t.Fatal(err)
				}
			}
			payload := map[string]any{
				"servicePath":    dir,
				"useContentHash": true,
				"fileInfos": []map[string]any{{
					"FileRelativePath": "app.dll",
					"FileSize":         tt.sourceSize,
					"ModifyTimeStamp":  sourceTime.Unix(),
					"ModifyTimeNano":   sourceTime.UnixNano(),
					"ContentHash":      tt.hash,
				}},
			}
			body, err := json.Marshal(payload)
			if err != nil {
				t.Fatal(err)
			}
			app := fiber.New()
			app.Post("/compare-files", CompareFiles)
			req := httptest.NewRequest(http.MethodPost, "/compare-files", bytes.NewReader(body))
			req.Header.Set("Content-Type", "application/json")
			resp, err := app.Test(req)
			if err != nil {
				t.Fatal(err)
			}
			defer resp.Body.Close()
			if resp.StatusCode != http.StatusOK {
				t.Fatalf("status = %d, want 200", resp.StatusCode)
			}
			var result struct {
				FileInfos             []dto.CompareFileInfo `json:"fileInfos"`
				HashRequiredFileInfos []dto.CompareFileInfo `json:"hashRequiredFileInfos"`
				SupportsContentHash   bool                  `json:"supportsContentHash"`
			}
			if err := json.NewDecoder(resp.Body).Decode(&result); err != nil {
				t.Fatal(err)
			}
			if len(result.FileInfos) != tt.wantUpdates || len(result.HashRequiredFileInfos) != tt.wantHash {
				t.Errorf("updates = %d, hash candidates = %d; want %d, %d", len(result.FileInfos), len(result.HashRequiredFileInfos), tt.wantUpdates, tt.wantHash)
			}
			if !result.SupportsContentHash {
				t.Error("agent did not advertise content hash support")
			}
		})
	}
}

func TestCompareFilesHashFailureRequiresUpdate(t *testing.T) {
	dir := t.TempDir()
	if err := os.WriteFile(filepath.Join(dir, "app.dll"), []byte("abc"), 0644); err != nil {
		t.Fatal(err)
	}
	original := targetFileHashes
	targetFileHashes = failingFileHasher{}
	t.Cleanup(func() { targetFileHashes = original })
	body, err := json.Marshal(map[string]any{
		"servicePath":    dir,
		"useContentHash": true,
		"fileInfos": []map[string]any{{
			"FileRelativePath": "app.dll", "FileSize": 3,
			"ContentHash": "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad",
		}},
	})
	if err != nil {
		t.Fatal(err)
	}
	app := fiber.New()
	app.Post("/compare-files", CompareFiles)
	req := httptest.NewRequest(http.MethodPost, "/compare-files", bytes.NewReader(body))
	req.Header.Set("Content-Type", "application/json")
	resp, err := app.Test(req)
	if err != nil {
		t.Fatal(err)
	}
	defer resp.Body.Close()
	if resp.StatusCode != http.StatusOK {
		t.Fatalf("target hash failure blocked comparison: status=%d", resp.StatusCode)
	}
	var result dto.CompareFilesResponse
	if err := json.NewDecoder(resp.Body).Decode(&result); err != nil {
		t.Fatal(err)
	}
	if len(result.FileInfos) != 1 || result.FileInfos[0].FileRelativePath != "app.dll" {
		t.Fatalf("unverified target file must be updated: %+v", result.FileInfos)
	}
}

type failingFileHasher struct{}

func (failingFileHasher) Hash(string) (string, error) {
	return "", errors.New("file changed while hashing")
}

func TestCompareFilesLegacyTimestampTolerance(t *testing.T) {
	for _, offset := range []time.Duration{0, 5 * time.Second, 6 * time.Second, -6 * time.Second} {
		t.Run(offset.String(), func(t *testing.T) {
			dir := t.TempDir()
			path := filepath.Join(dir, "app.dll")
			if err := os.WriteFile(path, []byte("abc"), 0644); err != nil {
				t.Fatal(err)
			}
			modTime := time.Unix(1700000000, 0)
			if err := os.Chtimes(path, modTime, modTime); err != nil {
				t.Fatal(err)
			}
			body, err := json.Marshal(dto.CompareFilesRequest{
				ServicePath: dir,
				FileInfos: []dto.CompareFileInfo{{
					FileRelativePath: "app.dll", FileSize: 3,
					ModifyTimeStamp: modTime.Add(offset).Unix(),
				}},
			})
			if err != nil {
				t.Fatal(err)
			}
			app := fiber.New()
			app.Post("/compare-files", CompareFiles)
			req := httptest.NewRequest(http.MethodPost, "/compare-files", bytes.NewReader(body))
			req.Header.Set("Content-Type", "application/json")
			resp, err := app.Test(req)
			if err != nil {
				t.Fatal(err)
			}
			defer resp.Body.Close()
			if resp.StatusCode != http.StatusOK {
				t.Fatalf("status = %d, want 200", resp.StatusCode)
			}
			var result dto.CompareFilesResponse
			if err := json.NewDecoder(resp.Body).Decode(&result); err != nil {
				t.Fatal(err)
			}
			wantUpdates := 0
			if offset > 5*time.Second || offset < -5*time.Second {
				wantUpdates = 1
			}
			if len(result.FileInfos) != wantUpdates || len(result.HashRequiredFileInfos) != 0 {
				t.Fatalf("legacy result = %+v, want %d updates without hashing", result, wantUpdates)
			}
		})
	}
}
