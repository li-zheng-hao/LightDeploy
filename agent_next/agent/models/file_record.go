package models

import (
	"fmt"

	"lightdeploy.agentclient/db"
)

// FileRecord 表示文件MD5记录
type FileRecord struct {
	Id                int    `json:"id"`
	ServiceName       string `json:"serviceName"`       // 服务名
	PublishTimestamp  int64  `json:"publishTimestamp"`  // 发布时间戳
	RelativeDirectory string `json:"relativeDirectory"` // 相对路径文件夹(服务所在文件夹或发布目录)
	AbsoluteDirectory string `json:"absoluteDirectory"` // 绝对路径文件夹
	FileName          string `json:"fileName"`          // 文件名
	MD5               string `json:"md5"`               // 文件MD5
}

func GetFileRecordsByServiceName(serviceName string) ([]FileRecord, error) {
	var fileRecords []FileRecord
	rows, err := db.DB.Query(`
		SELECT id, service_name, publish_timestamp, relative_directory, 
			   absolute_directory, file_name, md5 
		FROM file_records 
		WHERE service_name = ?`, serviceName)
	if err != nil {
		return nil, fmt.Errorf("获取%s文件记录失败: %v", serviceName, err)
	}
	defer rows.Close()
	for rows.Next() {
		var fileRecord FileRecord
		err := rows.Scan(
			&fileRecord.Id,
			&fileRecord.ServiceName,
			&fileRecord.PublishTimestamp,
			&fileRecord.RelativeDirectory,
			&fileRecord.AbsoluteDirectory,
			&fileRecord.FileName,
			&fileRecord.MD5)
		if err != nil {
			return nil, fmt.Errorf("获取%s文件记录失败: %v", serviceName, err)
		}
		fileRecords = append(fileRecords, fileRecord)
	}
	if err = rows.Err(); err != nil {
		return nil, fmt.Errorf("迭代%s文件记录失败: %v", serviceName, err)
	}
	return fileRecords, nil
}
