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
    
    /// <summary>
    /// 目标文件夹 如果填了这个则不会通过ServiceName来找目标文件夹
    /// </summary>
    public string? TargetDir { get; set; }
    
    /// <summary>
    /// 是否仅复制文件，不启动和停止服务
    /// </summary>
    public bool? OnlyCopyFiles { get; set; }
    
}