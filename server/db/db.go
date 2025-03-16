package db

import (
	"fmt"
	"time"

	"ld_server/model" // 导入 model 包

	_ "modernc.org/sqlite"
	"xorm.io/xorm"
)

var Engine *xorm.Engine

func Init() error {
	var err error
	Engine, err = xorm.NewEngine(Database.Type, 
		Database.Path + Database.ConnectOptions)
	if err != nil {
		return fmt.Errorf("初始化数据库失败: %v", err)
	}

	// 基本配置
	Engine.SetMaxIdleConns(Database.MaxIdleConns)
	Engine.SetMaxOpenConns(Database.MaxOpenConns)
	Engine.ShowSQL(Database.ShowSQL)

	// SQLite 优化配置
	// 设置连接生命周期
	Engine.SetConnMaxLifetime(time.Minute * 5)
	// 设置空闲连接超时
	Engine.SetConnMaxIdleTime(time.Minute)

	// 同步所有表结构
	if err := syncTables(); err != nil {
		return fmt.Errorf("同步数据库表结构失败: %v", err)
	}

	return nil
}

// 同步所有表结构
func syncTables() error {
	return Engine.Sync2(
		new(model.DeployService),
		new(model.DeployTarget),
		new(model.DeployHistory),
	)
}