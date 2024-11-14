package main

import (
	"context"
	"fmt"
	"net/http"
	"os"
	"os/signal"
	"syscall"
	"time"

	"lightdeploy.agentclient/config"
	"lightdeploy.agentclient/db"
	"lightdeploy.agentclient/log"
	"lightdeploy.agentclient/route"
)

func main() {
	startTime := time.Now()
	// 初始化日志
	log.Init("./logs/agent")

	config.InitConfig("config/config.yaml")
	log.Info("服务器配置: %+v\n", config.GetConfig().Server)

	// 初始化数据库
	go func() {
		err := db.InitDB("lightdeploy_agent_v3.db")
		if err != nil {
			log.Error("初始化数据库失败, 程序退出: %v", err)
			os.Exit(1)
		}
	}()

	// 初始化http服务
	server := &http.Server{
		Addr:    fmt.Sprintf("%s:%d", config.GetConfig().Server.Host, config.GetConfig().Server.Port),
		Handler: route.SetupRoutes(),
	}

	log.Info("启动HTTP服务器于: %s:%d, 耗时: %s", config.GetConfig().Server.Host, config.GetConfig().Server.Port, time.Since(startTime))

	// 添加信号处理
	stop := make(chan os.Signal, 1)
	signal.Notify(stop, os.Interrupt, syscall.SIGTERM)

	go func() {
		if err := server.ListenAndServe(); err != nil && err != http.ErrServerClosed {
			log.Error("HTTP服务器启动失败: %v", err)
		}
	}()

	// 等待中断信号
	<-stop
	log.Info("正在关闭服务器...")

	// 创建一个5秒超时的上下文
	ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancel()

	// 优雅关闭服务器
	if err := server.Shutdown(ctx); err != nil {
		log.Error("服务器关闭出错: %v", err)
	}

	// 关闭数据库连接
	db.CloseDB()

	log.Info("服务器已关闭")
}
