package deploy_test

import (
	"archive/zip"
	"bytes"
	"encoding/json"
	"fmt"
	"io"
	"ld_server/db"
	"ld_server/model"
	"ld_server/router"
	"ld_shared/dto"
	"ld_shared/sse"
	"net"
	"net/http"
	"net/http/httptest"
	"os"
	"path/filepath"
	"strconv"
	"strings"
	"sync/atomic"
	"testing"

	"github.com/glebarez/sqlite"
	"github.com/gofiber/fiber/v2"
	"gorm.io/gorm"
	"gorm.io/gorm/logger"
)

// Exercise the real request, comparison, package upload and log retrieval with
// an isolated database and agent; no browser connection exists during publish.
func TestFastDeployRetainsFileListWhileBrowserDisconnected(t *testing.T) {
	directory := t.TempDir()
	project := filepath.Join(directory, "project")
	if err := os.Mkdir(project, 0755); err != nil {
		t.Fatal(err)
	}
	if err := os.WriteFile(filepath.Join(project, "app.dll"), []byte("updated content"), 0644); err != nil {
		t.Fatal(err)
	}
	previousDB := db.DB
	var err error
	db.DB, err = gorm.Open(sqlite.Open(filepath.Join(directory, "data.db")), &gorm.Config{Logger: logger.Default.LogMode(logger.Silent)})
	if err != nil {
		t.Fatal(err)
	}
	t.Cleanup(func() {
		connection, _ := db.DB.DB()
		connection.Close()
		db.DB = previousDB
	})
	if err := db.DB.AutoMigrate(&model.DeployService{}, &model.DeployTarget{}, &model.DeployHistory{}); err != nil {
		t.Fatal(err)
	}
	var deployments atomic.Int32
	agent := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		switch r.URL.Path {
		case "/api/deploy/compare-files":
			var request dto.CompareFilesRequest
			if err := json.NewDecoder(r.Body).Decode(&request); err != nil {
				t.Error(err)
				w.WriteHeader(http.StatusBadRequest)
				return
			}
			json.NewEncoder(w).Encode(dto.CompareFilesResponse{FileInfos: request.FileInfos, SupportsContentHash: true})
		case "/api/sse/sse":
			w.Header().Set("Content-Type", "text/event-stream")
			fmt.Fprint(w, "data: SSE连接已建立\n\n")
			w.(http.Flusher).Flush()
			<-r.Context().Done()
		case "/api/deploy/deploy-windows-service":
			file, _, err := r.FormFile("file")
			if err != nil {
				t.Error(err)
				w.WriteHeader(http.StatusBadRequest)
				return
			}
			defer file.Close()
			data, _ := io.ReadAll(file)
			archive, err := zip.NewReader(bytes.NewReader(data), int64(len(data)))
			if err != nil || len(archive.File) != 1 || archive.File[0].Name != "app.dll" {
				t.Errorf("uploaded package does not match changed file: %v", err)
				w.WriteHeader(http.StatusBadRequest)
				return
			}
			deployments.Add(1)
		default:
			w.WriteHeader(http.StatusNotFound)
		}
	}))
	defer agent.Close()
	host, portText, _ := net.SplitHostPort(strings.TrimPrefix(agent.URL, "http://"))
	port, _ := strconv.Atoi(portText)
	service := model.DeployService{ServiceName: "test", ProjectPath: project, OnlyCopyFile: true}
	if err := db.DB.Create(&service).Error; err != nil {
		t.Fatal(err)
	}
	target := model.DeployTarget{ServiceId: service.Id, Host: host, Port: port, ServicePath: "isolated-target"}
	if err := db.DB.Create(&target).Error; err != nil {
		t.Fatal(err)
	}
	app := fiber.New(fiber.Config{DisableStartupMessage: true})
	router.RegisterRoutes(app)
	response, err := app.Test(httptest.NewRequest(http.MethodPost, "/api/deploy/tasks", nil))
	if err != nil {
		t.Fatal(err)
	}
	var task struct {
		ID string `json:"taskId"`
	}
	json.NewDecoder(response.Body).Decode(&task)
	response.Body.Close()
	if response.StatusCode != http.StatusOK || task.ID == "" {
		t.Fatalf("could not prepare deployment: %+v", task)
	}
	_, _, disconnect, err := sse.DeploymentLogs.Subscribe(task.ID, 0)
	if err != nil {
		t.Fatal(err)
	}
	disconnect()
	body := fmt.Sprintf(`{"serviceId":%d,"targetIds":[%d],"useFastMode":true,"taskId":%q}`, service.Id, target.Id, task.ID)
	deployRequest := func() *http.Response {
		r := httptest.NewRequest(http.MethodPost, "/api/deploy/deploy-service", strings.NewReader(body))
		r.Header.Set("Content-Type", "application/json")
		response, err := app.Test(r, 5000)
		if err != nil {
			t.Fatal(err)
		}
		return response
	}
	response = deployRequest()
	data, _ := io.ReadAll(response.Body)
	response.Body.Close()
	if response.StatusCode != http.StatusOK {
		t.Fatalf("deployment failed: %s", data)
	}
	response, err = app.Test(httptest.NewRequest(http.MethodGet, "/api/deploy/tasks/"+task.ID+"/logs?after=0", nil))
	if err != nil {
		t.Fatal(err)
	}
	defer response.Body.Close()
	var logs sse.LogSnapshot
	if err := json.NewDecoder(response.Body).Decode(&logs); err != nil {
		t.Fatal(err)
	}
	fileListFound := false
	for _, event := range logs.Events {
		if strings.Contains(event.Message, "需要更新 1 个文件: app.dll") {
			fileListFound = true
		}
	}
	if !fileListFound || len(logs.Events) == 0 || logs.Events[len(logs.Events)-1].Message != "部署成功" {
		t.Fatalf("completed HTTP deployment lost its changed file list: %+v", logs)
	}
	response = deployRequest()
	response.Body.Close()
	if response.StatusCode == http.StatusOK || deployments.Load() != 1 {
		t.Fatal("reusing the deployment task caused a second publish")
	}
}
