using LightApi.EFCore.Entities;

namespace LightDeploy.Server.Domain;

public class DeployTarget:IEfEntity
{
    public int Id { get; set; }
    
    /// <summary>
    /// Agent Ip
    /// </summary>
    public string Host { get; set; }
    
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
    
    public int? ServiceId { get; set; }
    

}

