import { apiClient } from './apiClient'
// 部署目标类型定义
export interface DeployTarget {
    id: number;
    serviceId: number;
    host: string;
    port: number;
    secretKey?: string;
    servicePath?: string;
    comment?: string;
    // 程序执行路径
	exePath?: string;
    // 程序执行参数
    exeParams?: string;
    // 目标状态
    status?: string; //状态
    message?: string; //消息
}
// 获取部署目标列表
export const getTargetList = (serviceId?: number | string) => {
    const params = serviceId ? { serviceId } : {}
    return apiClient.get<DeployTarget[]>('/api/target/list', { params })
}

// 创建部署目标
export const createDeployTarget = (target: Omit<DeployTarget, 'id'>) => {
    return apiClient.post<DeployTarget>('/api/target/create', target)
}

// 更新部署目标
export const updateDeployTarget = (target: DeployTarget) => {
    return apiClient.post<{ message: string }>(`/api/target/update`, target)
}

// 删除部署目标
export const deleteDeployTarget = (id: number | string) => {
    return apiClient.post<{ message: string }>(`/api/target/delete/${id}`)
}

// 获取所有主机响应
export interface GetAllHostsResponse {
    host: string;
}

// 获取所有服务响应
export interface GetAllServiceResponse {
    host: string;
    serviceName: string;
    environment: string;
    groupName: string;
    port: number;
}

// 获取所有目标主机
export const getAllHosts = () => {
    return apiClient.get<GetAllHostsResponse[]>('/api/target/hosts');
}

// 获取指定目标主机的服务
export const getAllServices = (host?: string) => {
    const params = host ? { host } : {};
    return apiClient.get<GetAllServiceResponse[]>('/api/target/service', { params });
}
 