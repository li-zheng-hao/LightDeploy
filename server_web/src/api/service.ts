import { apiClient } from './apiClient'

// 服务类型定义
export interface DeployService {
    id: number;
    groupName: string;
    serviceName: string;
    projectPath: string;
    port: number;
    projectType: number;
    comment?: string;
    environment?: string;
    onlyCopyFile?: boolean;
}



export const ProjectTypeOptions = [
    { label: '.NET项目', value: 1 },
    { label: '文件夹', value: 2 },
]

// 目标状态
export interface TargetStatus {
    targetId?: number;
    status?: string;
    message?: string;
}


// 获取服务列表
export const getServiceList = () => {
    return apiClient.get<DeployService[]>('/api/service/list')
}

// 获取服务状态
export const getServiceStatus = (serviceId: number | string) => {
    return apiClient.get<TargetStatus[]|null>(`/api/service/status/${serviceId}`)
}

// 创建部署服务
export const createDeployService = (service: Omit<DeployService, 'id'>) => {
    return apiClient.post<{ affected: number; data: DeployService }>('/api/service/create', service)
}

// 更新部署服务
export const updateDeployService = (service: DeployService) => {
    return apiClient.post<{ affected: number }>('/api/service/update', service)
}

// 删除部署服务
export const deleteDeployService = (id: number | string) => {
    return apiClient.post<{ affected: number }>(`/api/service/delete/${id}`)
}

