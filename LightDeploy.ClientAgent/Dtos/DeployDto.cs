namespace LightDeploy.ClientAgent.Dto;

public class DeployDto
{
    /// <summary>
    /// zip压缩包
    /// </summary>
    public IFormFile File { get; set; }
    
    /// <summary>
    /// Windows服务名
    /// </summary>
    public string ServiceName { get; set; }
    
    /// <summary>
    /// 忽略文件的后缀名
    /// </summary>
    public List<string>? IgnoreFileExtensions { get; set; }
    
    /// <summary>
    /// 需要更新的文件信息 主要用于备份
    /// </summary>
    public string FileInfos { get; set; }
    
    /// <summary>
    /// 健康检查地址
    /// </summary>
    public string? HealthCheckUrl { get; set; }
    
    /// <summary>
    /// signalR连接Id
    /// </summary>
    public string ConnectionId { get; set; }
    
}