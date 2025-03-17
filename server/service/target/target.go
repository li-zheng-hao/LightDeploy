package target

import (
	"encoding/json"
	"fmt"
	"ld_server/db"
	"ld_server/model"
	"net/http"
	"sync"
	"time"
)

// 收集所有目标机器的服务状态
type TargetStatus struct {
	TargetId int    `json:"targetId"`
	Status   string `json:"status"`
	Message  string `json:"message"`
}

func GetServiceStatus(serviceId int) ([]TargetStatus, error) {
	service := new(model.DeployService)
	_, err := db.Engine.ID(serviceId).Get(service)
	if err != nil {
		return nil, err
	}

	targets := make([]model.DeployTarget, 0)
	err = db.Engine.Where("service_id = ?", serviceId).Find(&targets)
	if err != nil {
		return nil, err
	}

	// 创建一个带超时的HTTP客户端
	client := &http.Client{
		Timeout: 5 * time.Second,
	}

	var wg sync.WaitGroup
	var mu sync.Mutex
	statuses := make([]TargetStatus, 0, len(targets))

	for _, target := range targets {
		wg.Add(1)
		go func(t model.DeployTarget) {
			defer wg.Done()

			// 构建请求URL
			url := fmt.Sprintf("http://%s:%d/api/service/get-windows-service-status?serviceName=%s",
				t.Host, t.Port, service.ServiceName)

			// 发送HTTP GET请求
			resp, err := client.Get(url)
			if err != nil {
				mu.Lock()
				statuses = append(statuses, TargetStatus{
					TargetId: t.Id,
					Status:   "error",
					Message:  "请求失败: " + err.Error(),
				})
				mu.Unlock()
				return
			}
			defer resp.Body.Close()

			// 解析响应
			var result struct {
				Status  string `json:"status"`
				Message string `json:"message"`
			}
			if err := json.NewDecoder(resp.Body).Decode(&result); err != nil {
				mu.Lock()
				statuses = append(statuses, TargetStatus{
					TargetId: t.Id,
					Status:   "error",
					Message:  "解析响应失败: " + err.Error(),
				})
				mu.Unlock()
				return
			}

			mu.Lock()
			statuses = append(statuses, TargetStatus{
				TargetId: t.Id,
				Status:   result.Status,
				Message:  result.Message,
			})
			mu.Unlock()
		}(target)
	}

	wg.Wait()
	return statuses, nil
}
