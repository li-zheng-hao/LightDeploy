package model

type DeployService struct {
	Id int `gorm:"column:id;primaryKey;autoIncrement" json:"id"`
	// 组名
	GroupName string `gorm:"column:group_name;type:text;not null;index" json:"groupName"`
	// 服务名
	ServiceName string `gorm:"column:service_name;type:text;not null" json:"serviceName"`
	// 项目路径
	ProjectPath string `gorm:"column:project_path;type:text;not null" json:"projectPath"`
	// 端口
	Port int `gorm:"column:port;type:integer;not null" json:"port"`
	// 项目类型
	// 1 .NET Core Projet项目 2. 文件夹发布
	ProjectType int `gorm:"column:project_type;type:integer;not null" json:"projectType"`
	// 备注说明
	Comment string `gorm:"column:comment;type:text" json:"comment"`
	// 环境 开发、测试、预发、生产等
	Environment string `gorm:"column:environment;type:text" json:"environment"`
	// 是否只复制文件 不启动和停止服务 0 否 1 是
	OnlyCopyFile bool `gorm:"column:only_copy_file;type:boolean" json:"onlyCopyFile"`
	// 忽略文件列表，支持正则表达式，多个用|分割
	IgnoreFileRegex string `gorm:"column:ignore_file_regex;type:text" json:"ignoreFileRegex"`
}

const (
	ProjectTypeNetCore = 1
	ProjectTypeFolder  = 2
)

// TableName 指定表名
func (DeployService) TableName() string {
	return "deploy_service"
}
