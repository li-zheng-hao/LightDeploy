import { defineStore } from 'pinia'

interface DeployState {
  // 选择的服务id
  deployServiceId: number | null
  // 分组名称
  groupName: string | null
  // 环境名称
  environment: string | null
}

export const useDeployStore = defineStore({
  id: 'deploy-store',
  state: (): DeployState => ({
    deployServiceId: null,
    groupName: null,
    environment: null
  }),
  getters: {},
  actions: {},
  persist: true
})
