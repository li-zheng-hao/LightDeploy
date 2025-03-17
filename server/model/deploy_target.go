package model

type DeployTarget struct {
	Id int `xorm:"pk autoincr 'id'" json:"id"`
	// 服务id
	ServiceId int `xorm:"INTEGER notnull 'service_id'" json:"serviceId"`
	// 目标主机
	Host string `xorm:"TEXT notnull 'host'" json:"host"`
	// 目标端口
	Port int `xorm:"INTEGER notnull 'port'" json:"port"`
	// 密钥
	SecretKey string `xorm:"TEXT 'secret_key'" json:"secretKey"`
	// 服务路径 文件夹路径
	ServicePath string `xorm:"TEXT 'service_path'" json:"servicePath"`
	// 备注
	Comment string `xorm:"TEXT 'comment'" json:"comment"`
	// 程序执行完整路径
	ExePath string `xorm:"TEXT 'exe_path'" json:"exePath"`
	// 程序执行参数
	ExeParams string `xorm:"TEXT 'exe_params'" json:"exeParams"`
}
