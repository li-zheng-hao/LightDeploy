package model

type DeployTarget struct {
	Id int `gorm:"primaryKey;autoIncrement" json:"id"`
	// 服务id
	ServiceId int `gorm:"type:integer;not null" json:"serviceId"`
	// 目标主机
	Host string `gorm:"type:text;not null" json:"host"`
	// 目标端口
	Port int `gorm:"type:integer;not null" json:"port"`
	// 密钥
	SecretKey string `gorm:"type:text" json:"secretKey"`
	// 服务路径 文件夹路径
	ServicePath string `gorm:"type:text" json:"servicePath"`
	// 备注
	Comment string `gorm:"type:text" json:"comment"`
	// 程序执行完整路径
	ExePath string `gorm:"type:text" json:"exePath"`
	// 程序执行参数
	ExeParams string `gorm:"type:text" json:"exeParams"`
}

// TableName 指定表名
func (DeployTarget) TableName() string {
	return "deploy_target"
}
