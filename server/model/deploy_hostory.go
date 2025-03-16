package model

import "time"

type DeployHistory struct {
	Id int `xorm:"pk autoincr 'id'" json:"id"`
	// 部署服务
	ServiceId int `xorm:"INTEGER notnull 'service_id'" json:"serviceId"`
	// 部署时间
	DeployTime time.Time `xorm:"DATETIME notnull 'deploy_time'" json:"deployTime"`
	// 备注
	Comment string `xorm:"TEXT 'comment'" json:"comment"`
}
