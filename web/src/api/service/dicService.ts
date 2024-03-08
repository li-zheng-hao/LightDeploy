import { apiClient } from '../client/apiClient'
/**
 *  获取环境列表
 * @returns
 */
export async function getEnvironments():Promise<[]> {
  const res=await apiClient
    .request<string[]>({
      url: '/dic/environments',
      method: 'get'
    })
  return res.map((item) => ({ label: item, value: item }))
}
