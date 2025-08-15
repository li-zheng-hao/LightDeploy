package db

import (
	"fmt"
	"time"

	"ld_server/model" // 导入 model 包

	"github.com/glebarez/sqlite"
	"gorm.io/gorm"
	"gorm.io/gorm/logger"
)

var DB *gorm.DB

func Init() error {
	var err error

	// 配置日志级别
	logLevel := logger.Silent
	if Database.ShowSQL {
		logLevel = logger.Info
	}

	DB, err = gorm.Open(sqlite.Open(Database.Path+Database.ConnectOptions), &gorm.Config{
		Logger: logger.Default.LogMode(logLevel),
	})
	if err != nil {
		return fmt.Errorf("初始化数据库失败: %v", err)
	}

	// 获取底层的sql.DB对象来设置连接池
	sqlDB, err := DB.DB()
	if err != nil {
		return fmt.Errorf("获取数据库连接失败: %v", err)
	}

	// 基本配置
	sqlDB.SetMaxIdleConns(Database.MaxIdleConns)
	sqlDB.SetMaxOpenConns(Database.MaxOpenConns)
	// 设置连接生命周期
	sqlDB.SetConnMaxLifetime(time.Minute * 5)
	// 设置空闲连接超时
	sqlDB.SetConnMaxIdleTime(time.Minute)

	// 同步所有表结构
	if err := syncTables(); err != nil {
		return fmt.Errorf("同步数据库表结构失败: %v", err)
	}

	return nil
}

// 同步所有表结构
func syncTables() error {
	return DB.AutoMigrate(
		&model.DeployService{},
		&model.DeployTarget{},
		&model.DeployHistory{},
	)
}
