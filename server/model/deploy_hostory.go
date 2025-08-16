package model

import "time"

type DeployHistory struct {
	Id int `gorm:"column:id;primaryKey;autoIncrement" json:"id"`
	// 部署服务
	ServiceId int `gorm:"column:service_id;type:integer;not null" json:"serviceId"`
	// 部署时间
	DeployTime time.Time `gorm:"column:deploy_time;type:datetime;not null" json:"deployTime"`
	// 备注
	Comment string `gorm:"column:comment;type:text" json:"comment"`
}

// TableName 指定表名
func (DeployHistory) TableName() string {
	return "deploy_history"
}
