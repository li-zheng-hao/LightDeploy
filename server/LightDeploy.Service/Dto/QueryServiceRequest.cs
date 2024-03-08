namespace LightDeploy.Service.Dto;

public class QueryServiceRequest
{
    /// <summary>
    /// 服务类别
    /// </summary>
    public string? ServiceGroup { get; set; }
    /// <summary>
    /// 服务名
    /// </summary>
    public string? ServiceName { get; set; }
    /// <summary>
    /// 服务环境
    /// </summary>
    public string? Environment { get; set; }
}