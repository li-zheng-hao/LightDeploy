// index.ts
import axios from 'axios'
import type { AxiosError, AxiosInstance, AxiosRequestConfig, AxiosResponse } from 'axios'
import { handleHttpError } from './httpErrorHandler'
import { deepClone, deepMerge } from '@/utils'
import { handleBusinessError } from '@/api/client/businessErrorHandler'
import { useJwt } from './jwtAuth'
import { generateRequestKey } from './helper'
import idmp, { type IdmpOptions } from 'idmp'
import { error } from 'console'
import qs from 'qs'
export type ApiResult<T> = {
  code: number
  msg: string
  data: T
  success: boolean
}

export interface RequestConfig {
  /**
   * 弹窗展示异常
   */
  showError?: boolean | undefined | null
  /**
   * 直接返回响应的data数据
   */
  unwrapResult?: boolean | undefined | null
  /**
   * 是否返回原始的Axios响应
   */
  returnRawAxiosResponse?: boolean | undefined | null
  /**
   * 是否抛出业务异常错误
   */
  throwBusinessError?: boolean | undefined | null

  /**
   * 是否使用idmp包裹请求 用于处理幂等和防重复
   */
  useIdmp?: boolean | undefined | null

  /**
   * 是否在请求之前刷新idmp请求key
   */
  refreshIdmpRequestKey?: boolean | undefined | null

  /**
   * idmp相关配置
   */
  idmpOptions?: IdmpOptions | undefined | null
}
export class ApiClient {
  /**
   * axios 实例
   */
  axiosInstance: AxiosInstance
  /**
   * 基础配置，url和超时时间
   */
  baseConfig: AxiosRequestConfig = {
    baseURL: '/api',
    timeout: 60000,
    // 处理query参数中的数组 变成a[]=1&a[]=2导致后端无法解析的问题
    paramsSerializer: (params) => {
      return qs.stringify(params)
    }
  }
  /**
   * 默认请求配置
   */
  defaultRequestConfig: RequestConfig = {}

  constructor(config: AxiosRequestConfig, requestConfig?: RequestConfig) {
    this.defaultRequestConfig = Object.assign(this.defaultRequestConfig, requestConfig ?? {})
    // 使用axios.create创建axios实例
    this.axiosInstance = axios.create(Object.assign(this.baseConfig, config))

    // 改为使用axios-jwt库进行token的管理
    // this.axiosInstance.interceptors.request.use(
    //     (config: any) => {
    //         // 一般会请求拦截里面加token，用于后端的验证
    //         const token = localStorage.getItem("token") as string
    //         if(token) {
    //             config.headers!.Authorization = token;
    //         }
    //         return config;
    //     },
    //     (err: any) => {
    //         // 请求错误，这里可以用全局提示框进行提示
    //         return Promise.reject(err);
    //     }
    // );

    this.axiosInstance.interceptors.response.use(undefined, (err: AxiosError) => {
      handleHttpError(err as any)
      return Promise.reject(err)
    })
  }

  public getAxios(): AxiosInstance {
    return this.axiosInstance
  }

  /**
   * 请求
   * @param config axios配置
   * @param requestConfig 自定义请求配置
   */
  public async request<T>(config: AxiosRequestConfig, requestConfig?: RequestConfig): Promise<T> {
    const targetConfig = deepMerge<RequestConfig>(this.defaultRequestConfig, requestConfig)

    let requestKey = null
    // 如果需要刷新idmp请求key
    if (targetConfig.refreshIdmpRequestKey) {
      requestKey ??= generateRequestKey(config)
      idmp.flush(requestKey)
    }
    // 使用idmp请求
    if (targetConfig.useIdmp) {
      requestKey ??= generateRequestKey(config)
      let result = await idmp(
        requestKey,
        () => this.internalRequest<T>(config, targetConfig),
        targetConfig.idmpOptions ?? undefined
      )
      // https://github.com/ha0z1/idmp/blob/c28937d9ce54ab63f7272a202b11530ed439a451/src/index.ts#L84
      // 必须深拷贝结果，因为idmp缓存的数据设置了只读
      return deepClone(result)
    }
    // 直接使用axios请求
    return this.internalRequest<T>(config, targetConfig)
  }

  /**
   * 内部调用
   * @param config
   * @param requestConfig
   * @returns
   */
  private internalRequest<T>(config: AxiosRequestConfig, requestConfig: RequestConfig): Promise<T> {
    return new Promise((resolve, reject) => {
      this.axiosInstance.request<any, AxiosResponse<ApiResult<T>>>(config).then(
        (res: AxiosResponse<ApiResult<T>>) => {
          if (res.data === null || res.data === undefined || res.data == '')
            return resolve(res as any)
          if (res.data?.success===false) {
            const err=handleBusinessError(res.data, requestConfig)
            if(err) reject(err)
          }
          if (res.data.success===true && requestConfig.unwrapResult) return resolve(res.data.data)
          else if (requestConfig?.returnRawAxiosResponse) return resolve(res as any)
          else return resolve(res.data as any)
        }
      )
    })
  }
}

// 如果有需要可以配置多个
const apiClient = new ApiClient(
  {},
  {
    showError: true,
    unwrapResult: true,
    returnRawAxiosResponse: false,
    throwBusinessError: true,
    useIdmp: true,
    idmpOptions: {
      maxRetry: 0,
      maxAge: 500
    }
  }
)

// useJwt(apiClient)

export { apiClient }
