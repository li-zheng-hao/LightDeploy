package model

type DeployTarget struct {
	Id int `gorm:"column:id;primaryKey;autoIncrement" json:"id"`
	// 服务id
	ServiceId int `gorm:"column:service_id;type:integer;not null" json:"serviceId"`
	// 目标主机
	Host string `gorm:"column:host;type:text;not null" json:"host"`
	// 目标端口
	Port int `gorm:"column:port;type:integer;not null" json:"port"`
	// 密钥
	SecretKey string `gorm:"column:secret_key;type:text" json:"secretKey"`
	// 服务路径 文件夹路径
	ServicePath string `gorm:"column:service_path;type:text" json:"servicePath"`
	// 备注
	Comment string `gorm:"column:comment;type:text" json:"comment"`
	// 程序执行完整路径
	ExePath string `gorm:"column:exe_path;type:text" json:"exePath"`
	// 程序执行参数
	ExeParams string `gorm:"column:exe_params;type:text" json:"exeParams"`
}

// TableName 指定表名
func (DeployTarget) TableName() string {
	return "deploy_target"
}
