package model

import "time"

type DeployHistory struct {
	Id int `gorm:"primaryKey;autoIncrement" json:"id"`
	// 部署服务
	ServiceId int `gorm:"type:integer;not null" json:"serviceId"`
	// 部署时间
	DeployTime time.Time `gorm:"type:datetime;not null" json:"deployTime"`
	// 备注
	Comment string `gorm:"type:text" json:"comment"`
}

// TableName 指定表名
func (DeployHistory) TableName() string {
	return "deploy_history"
}
