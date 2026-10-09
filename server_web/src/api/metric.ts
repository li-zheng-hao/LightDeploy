import { apiClient } from './apiClient'

// 操作系统分布
export interface OsCount {
    windows: number
    linux: number
    other: number
}

// 环境分布
export interface EnvironmentCount {
    environment: string
    count: number
}

// 状态汇总
export interface StatusSummary {
    running: number
    stopped: number
    notInstalled: number
    error: number
}

// 单个服务状态汇总
export interface ServiceStatus {
    serviceId: number
    serviceName: string
    environment: string
    groupName: string
    running: number
    stopped: number
    notInstalled: number
    error: number
}

// 仪表盘概览
export interface Overview {
    serviceCount: number
    hostCount: number
    agentCount: number
    deployCount: number
    osCount: OsCount
    environmentCount: EnvironmentCount[]
    statusSummary: StatusSummary
    services: ServiceStatus[]
}

// 获取仪表盘概览数据
export const getOverview = () => {
    return apiClient.get<Overview>('/api/metric/overview')
}