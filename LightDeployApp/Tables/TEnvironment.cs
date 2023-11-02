namespace LightDeployApp.Tables;

public class TEnvironment
{
    public string Name { get; set; }
    
    public string Host { get; set; }
    
    public string Port { get; set; }
    
    /// <summary>
    /// 健康检查Url
    /// </summary>
    public string? HealthCheckUrl { get; set; }
    
}