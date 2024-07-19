using LightDeploy.Server.Domain;

namespace LightDeploy.Server.ViewModels;

public class DeployTargetViewModel
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
    
    public int ServiceId { get; set; }
    
    public string? Status { get; set; }
}