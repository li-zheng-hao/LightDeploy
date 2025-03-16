import { apiClient } from './apiClient'
import type { DeployTarget } from './target'

// Agent相关接口返回类型定义
interface VersionInfo {
  version: string
}

// Agent相关API
export const getAgentVersion = (targetId:number) => {
  return apiClient.get<VersionInfo>('/api/agent/version', {
    params: {
      targetId: targetId
    }
  })
}

export const getAllAgent = () => {
  return apiClient.get<DeployTarget[]>('/api/agent/all')
}

interface UpdateAgentParams {
  file: File
  agentServiceName: string
  targetId: number
}

export const updateAgent = (params: UpdateAgentParams) => {
  const formData = new FormData()
  formData.append('file', params.file)
  formData.append('agentServiceName', params.agentServiceName)
  formData.append('targetId', params.targetId.toString())
  
  return apiClient.post('/api/agent/update', formData, {
    headers: {
      'Content-Type': 'multipart/form-data'
    }
  })
}