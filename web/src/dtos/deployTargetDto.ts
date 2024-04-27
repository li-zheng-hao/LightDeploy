export interface DeployTargetDto {
  id?: number|undefined;

  /**
   * Agent Ip
   */
  host?: string|undefined;

  /**
   * Agent端口号
   */
  port?: string|undefined;

  /**
   * 健康检查Url
   */
  healthCheckUrl?: string|undefined; // 使用问号表示可选属性

  /**
   * Token
   */
  authKey?: string|undefined; // 使用问号表示可选属性

  /**
   * 服务Id
   */
  serviceId?: number | null |undefined; // 使用 number | null 表示可以为 null 的数字类型

  status?:string|null|undefined;// 状态
}
