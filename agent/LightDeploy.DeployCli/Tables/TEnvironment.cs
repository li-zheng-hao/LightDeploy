using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LightDeploy.DeployCli.Tables;

public class TEnvironment
{
    public string Name { get; set; }
    
    public string Host { get; set; }
    
    public string Port { get; set; }
    
    /// <summary>
    /// 健康检查Url
    /// </summary>
    public string? HealthCheckUrl { get; set; }
    
    /// <summary>
    /// Token
    /// </summary>
    public string? AuthKey { get; set; }
}