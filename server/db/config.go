package db

import (
	"ld_shared/env"
	"log/slog"
	"os"
	"path/filepath"
)

type DatabaseConfig struct {
	Type           string
	Path           string
	MaxIdleConns   int
	MaxOpenConns   int
	ShowSQL        bool
	ConnectOptions string
}

func init() {
	execPath, err := os.Executable()
	if err != nil {
		panic(err)
	}
	if !env.IsDebugMode() {
		execDir := filepath.Dir(execPath)
		Database.Path = filepath.Join(execDir, "data.db")
	}
	slog.Debug("数据库路径", "path", Database.Path)
}

var Database = DatabaseConfig{
	Type: "sqlite",
	Path: "./data.db", // 这个默认值会被 init() 函数覆盖
	// SQLite 是单文件数据库，不需要太多连接
	MaxIdleConns: 1,
	MaxOpenConns: 1,
	ShowSQL:      true,
	// 添加 SQLite 特定配置
	ConnectOptions: "?_journal=WAL&_timeout=5000",
}