package metric

import (
	"encoding/json"
	"fmt"
	"io"
	"net/http"
	"net/url"
	"sort"
	"strings"
	"sync"
	"time"

	"ld_server/db"
	"ld_server/model"
)

const (
	// 单个目标机状态查询超时
	statusQueryTimeout = 5 * time.Second
	// 并发查询目标机状态的最大数量
	maxConcurrentStatusQueries = 30
)

// OsCount 目标机操作系统分布
type OsCount struct {
	Windows int `json:"windows"`
	Linux   int `json:"linux"`
	Other   int `json:"other"`
}

// EnvironmentCount 环境分布
type EnvironmentCount struct {
	Environment string `json:"environment"`
	Count       int    `json:"count"`
}

// StatusSummary 所有服务在所有目标机上的状态汇总
type StatusSummary struct {
	Running      int `json:"running"`
	Stopped      int `json:"stopped"`
	NotInstalled int `json:"notInstalled"`
	Error        int `json:"error"`
}

// ServiceStatus 单个服务的状态汇总
type ServiceStatus struct {
	ServiceId    int    `json:"serviceId"`
	ServiceName  string `json:"serviceName"`
	Environment  string `json:"environment"`
	GroupName    string `json:"groupName"`
	Running      int    `json:"running"`
	Stopped      int    `json:"stopped"`
	NotInstalled int    `json:"notInstalled"`
	Error        int    `json:"error"`
}

// Overview 仪表盘概览数据
type Overview struct {
	ServiceCount     int                `json:"serviceCount"`
	HostCount        int                `json:"hostCount"`
	AgentCount       int                `json:"agentCount"`
	DeployCount      int64              `json:"deployCount"`
	OsCount          OsCount            `json:"osCount"`
	EnvironmentCount []EnvironmentCount `json:"environmentCount"`
	StatusSummary    StatusSummary      `json:"statusSummary"`
	Services         []ServiceStatus    `json:"services"`
}

type statusJob struct {
	serviceIdx  int
	target      model.DeployTarget
	serviceName string
}

// GetOverview 汇总仪表盘所需的全部数据
func GetOverview() (*Overview, error) {
	overview := &Overview{
		Services:         []ServiceStatus{},
		EnvironmentCount: []EnvironmentCount{},
	}

	var services []model.DeployService
	if err := db.DB.Find(&services).Error; err != nil {
		return nil, fmt.Errorf("查询服务列表失败: %v", err)
	}

	var targets []model.DeployTarget
	if err := db.DB.Find(&targets).Error; err != nil {
		return nil, fmt.Errorf("查询目标列表失败: %v", err)
	}

	// 主机去重、操作系统分布
	hostSet := make(map[string]struct{})
	for _, t := range targets {
		hostSet[t.Host] = struct{}{}
		switch t.OsType {
		case model.OsTypeLinux:
			overview.OsCount.Linux++
		case model.OsTypeWindows, "":
			overview.OsCount.Windows++
		default:
			overview.OsCount.Other++
		}
	}

	// 环境分布
	envMap := make(map[string]int)
	for _, s := range services {
		env := s.Environment
		if strings.TrimSpace(env) == "" {
			env = "未指定"
		}
		envMap[env]++
	}
	for env, count := range envMap {
		overview.EnvironmentCount = append(overview.EnvironmentCount, EnvironmentCount{Environment: env, Count: count})
	}
	sort.Slice(overview.EnvironmentCount, func(i, j int) bool {
		return overview.EnvironmentCount[i].Count > overview.EnvironmentCount[j].Count
	})

	// 部署总数
	var deployCount int64
	if err := db.DB.Model(&model.DeployHistory{}).Count(&deployCount).Error; err != nil {
		return nil, fmt.Errorf("查询部署总数失败: %v", err)
	}

	overview.ServiceCount = len(services)
	overview.HostCount = len(hostSet)
	overview.AgentCount = len(hostSet)
	overview.DeployCount = deployCount

	// 初始化各服务状态结构
	serviceStatuses := make([]ServiceStatus, len(services))
	for i, s := range services {
		serviceStatuses[i] = ServiceStatus{
			ServiceId:   s.Id,
			ServiceName: s.ServiceName,
			Environment: s.Environment,
			GroupName:   s.GroupName,
		}
	}

	// 构建「服务 × 目标」任务列表
	jobs := make([]statusJob, 0)
	for i, s := range services {
		for _, t := range targets {
			if t.ServiceId != s.Id {
				continue
			}
			jobs = append(jobs, statusJob{serviceIdx: i, target: t, serviceName: s.ServiceName})
		}
	}

	// 限流并发查询各目标机状态
	var (
		wg      sync.WaitGroup
		mu      sync.Mutex
		summary StatusSummary
	)
	sem := make(chan struct{}, maxConcurrentStatusQueries)

	for _, job := range jobs {
		wg.Add(1)
		sem <- struct{}{}
		go func(j statusJob) {
			defer wg.Done()
			defer func() { <-sem }()

			category := queryTargetStatus(j.target, j.serviceName)

			mu.Lock()
			defer mu.Unlock()
			switch category {
			case "running":
				summary.Running++
				serviceStatuses[j.serviceIdx].Running++
			case "stopped":
				summary.Stopped++
				serviceStatuses[j.serviceIdx].Stopped++
			case "notInstalled":
				summary.NotInstalled++
				serviceStatuses[j.serviceIdx].NotInstalled++
			default:
				summary.Error++
				serviceStatuses[j.serviceIdx].Error++
			}
		}(job)
	}
	wg.Wait()

	overview.StatusSummary = summary
	overview.Services = serviceStatuses

	return overview, nil
}

// queryTargetStatus 查询单个目标机上某服务的状态，返回归类结果
// 归类：running / stopped / notInstalled / error
func queryTargetStatus(target model.DeployTarget, serviceName string) string {
	reqURL := fmt.Sprintf("http://%s:%d/api/service/get-windows-service-status?serviceName=%s",
		target.Host, target.Port, url.QueryEscape(serviceName))

	client := &http.Client{Timeout: statusQueryTimeout}
	resp, err := client.Get(reqURL)
	if err != nil {
		return "error"
	}
	defer resp.Body.Close()

	body, _ := io.ReadAll(resp.Body)

	if resp.StatusCode != http.StatusOK {
		msg := string(body)
		if strings.Contains(msg, "服务不存在") || strings.Contains(msg, "does not exist") || strings.Contains(msg, "not exist") {
			return "notInstalled"
		}
		return "error"
	}

	var result struct {
		Status string `json:"status"`
	}
	if err := json.Unmarshal(body, &result); err != nil {
		return "error"
	}

	switch {
	case strings.Contains(result.Status, "正在运行"):
		return "running"
	case strings.Contains(result.Status, "已停止"), strings.Contains(result.Status, "正在停止"):
		return "stopped"
	default:
		return "error"
	}
}