import { apiClient } from './apiClient'

// 部署历史记录类型
export interface DeployHistory {
  id: number // 修改为数字类型
  serviceId: number // 添加服务ID字段
  deployTime: string // 修改字段名为deployTime
  comment?: string // 修改message为comment
}

// 获取服务列表
export const getServiceList = () => {
  return apiClient.get<{ label: string; value: string }[]>('/api/deploy/services')
}

// 获取部署历史
export const getDeployHistory = (serviceId: string|number) => {
  return apiClient.get<DeployHistory[]>(`/api/history/${serviceId}`)
}

// 部署服务
export const deployService = (serviceId: number, targetIds: number[],comment:string,useFastMode:boolean) => {
  debugger;
  return apiClient.post('/api/deploy/deploy-service', { serviceId, targetIds,comment,useFastMode })
}


// 启动服务
export const startService = (serviceId: number, targetIds: number[]) => {
  return apiClient.post('/api/deploy/start-service', { serviceId, targetIds })
}

// 停止服务
export const stopService = (serviceId: number, targetIds: number[]) => {
  return apiClient.post('/api/deploy/stop-service', { serviceId, targetIds })
}

// 获取部署日志
export const getDeployLogs = (serviceId: number|string) => {
  return apiClient.get<DeployHistory[]|null>(`/api/history/${serviceId}`)
} 