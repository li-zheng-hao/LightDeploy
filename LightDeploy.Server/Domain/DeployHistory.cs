using LightApi.EFCore.Entities;

namespace LightDeploy.Server.Domain;

public class DeployHistory:IEfEntity
{
    public int Id { get; set; }
    
    /// <summary>
    /// 发布说明
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 发布时间
    /// </summary>
    public DateTime? PublishTime { get; set; }
    
    /// <summary>
    /// 服务id
    /// </summary>
    public int? ServiceId { get; set; }
}