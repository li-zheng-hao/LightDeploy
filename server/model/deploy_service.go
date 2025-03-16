package model

type DeployService struct {
	Id int `xorm:"pk autoincr INTEGER 'id'" json:"id"`
	// 组名
	GroupName string `xorm:"TEXT notnull index 'group_name'" json:"groupName"`
	// 服务名
	ServiceName string `xorm:"TEXT notnull unique 'service_name'" json:"serviceName"`
	// 项目路径
	ProjectPath string `xorm:"TEXT notnull 'project_path'" json:"projectPath"`
	// 端口
	Port int `xorm:"INTEGER notnull unique 'port'" json:"port"`
	// 项目类型
	// 1 .NET Core Projet项目 2. 文件夹发布
	ProjectType int `xorm:"INTEGER notnull 'project_type'" json:"projectType"`
	// 备注说明
	Comment string `xorm:"TEXT 'comment'" json:"comment"`
	// 环境 开发、测试、预发、生产等
	Environment string `xorm:"TEXT 'environment'" json:"environment"`
	// 是否只复制文件 不启动和停止服务 0 否 1 是
	OnlyCopyFile bool `xorm:"BOOLEAN 'only_copy_file'" json:"onlyCopyFile"`
}

const (
	ProjectTypeNetCore = 1
	ProjectTypeFolder  = 2
)
