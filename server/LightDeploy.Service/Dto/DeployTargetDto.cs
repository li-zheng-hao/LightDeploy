using LightApi.Infra.ModelValidator;

namespace LightDeploy.Service.Dto;

public class DeployTargetDto
{
    public int Id { get; set; }
    
    /// <summary>
    /// Agent Ip
    /// </summary>
    public string Host { get; set; }
    
    /// <summary>
    /// 服务端口
    /// </summary>
    public int ServicePort { get; set; }
    
    /// <summary>
    /// Agent端口号
    /// </summary>
    public string Port { get; set; }
    
    /// <summary>
    /// 健康检查Url
    /// </summary>
    public string? HealthCheckUrl { get; set; }
    
    /// <summary>
    /// Token
    /// </summary>
    public string? AuthKey { get; set; }
    
    /// <summary>
    /// 服务Id
    /// </summary>
    [RequiredEx]
    public int? ServiceId { get; set; }
}