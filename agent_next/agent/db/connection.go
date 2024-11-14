package db

import (
	"database/sql"

	"lightdeploy.agentclient/log"
	_ "modernc.org/sqlite"
)

var DB *sql.DB

// InitDB 初始化数据库连接
func InitDB(dbPath string) error {
	var err error
	DB, err = sql.Open("sqlite", dbPath)
	if err != nil {
		log.Error("连接数据库失败: %v", err)
		return err
	}

	// 测试数据库连接
	err = DB.Ping()
	if err != nil {
		log.Error("数据库连接测试失败: %v", err)
		return err
	}

	err = InitTables()

	if err != nil {
		log.Error("初始化数据库表失败: %v", err)
		return err
	}

	return nil
}

// InitTables 初始化数据库表
func InitTables() error {
	_, err := DB.Exec(`
		CREATE TABLE IF NOT EXISTS file_records (
			id INTEGER PRIMARY KEY AUTOINCREMENT,
			service_name TEXT,
			publish_timestamp INTEGER,
			relative_directory TEXT,
			absolute_directory TEXT,
			file_name TEXT,
			md5 TEXT
		);
		SELECT name FROM sqlite_master 
		WHERE type='index' AND name='unique_file_record';
		CREATE UNIQUE INDEX IF NOT EXISTS unique_file_record 
		ON file_records(service_name, relative_directory, file_name);
	`)
	if err != nil {
		log.Error("创建表失败: %v", err)
		return err
	}

	return nil
}

// CloseDB 关闭数据库连接
func CloseDB() {
	if DB != nil {
		DB.Close()
	}
}
