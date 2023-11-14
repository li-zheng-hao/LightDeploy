namespace LightDeploy.ClientAgent.Dto;

public class InstallWindowsServiceDto
{
    public IFormFile File { get; set; }
    
    public string ServiceName { get; set; }
    
    /// <summary>
    /// 运行参数
    /// </summary>
    public string? Params { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string ExeFullPath { get; set; }

    public string? ServiceDescription { get; set; }
    
    public string ConnectionId { get; set; }
}