import axios, { type AxiosInstance, type AxiosResponse, type AxiosError } from 'axios';
class ApiClient {
  private instance: AxiosInstance;

  constructor() {
    this.instance = axios.create({
      timeout: 300000,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    this.setupInterceptors();
  }

  private setupInterceptors(): void {
    // 请求拦截器
    this.instance.interceptors.request.use(
      (config) => {
        // 可以在这里添加token等认证信息
        const token = localStorage.getItem('token');
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error: AxiosError) => {
        return Promise.reject(error);
      }
    );

    // 响应拦截器
    this.instance.interceptors.response.use(
      (response: AxiosResponse) => {
        return response;
      },
      (error: AxiosError) => {
        if (error.response) {
          switch (error.response.status) {
            case 400:
              const errorMessage = (error.response.data as { message?: string }).message || '请求失败(400)';
              window.$message.error(errorMessage);
              break;
            case 401:
              window.$message.error('未授权，请重新登录(401)');
              // 可以在这里处理登出逻辑
              //   localStorage.removeItem('token');
              break;
            case 403:
              window.$message.error('权限不足(403)');
              break;
            case 404:
              window.$message.error('请求的资源不存在(404)');
              break;
            case 500:
              window.$message.error('服务器错误(500)');
              break;
            default:
              window.$message.error(`请求失败: ${error.response.status}`);
          }
        } else if (error.request) {
          window.$message.error('网络错误，请检查网络连接(error.request)');
        } else {
          window.$message.error('请求配置错误(error.request)');
        }
        return Promise.reject(error);
      }
    );
  }

  // 封装请求方法
  public get<T = any>(url: string, config = {}) {
    return this.instance.get<T>(url, config);
  }

  public post<T = any>(url: string, data = {}, config = {}) {
    return this.instance.post<T>(url, data, config);
  }

  public put<T = any>(url: string, data = {}, config = {}) {
    return this.instance.put<T>(url, data, config);
  }

  public delete<T = any>(url: string, config = {}) {
    return this.instance.delete<T>(url, config);
  }
}

export const apiClient = new ApiClient();

