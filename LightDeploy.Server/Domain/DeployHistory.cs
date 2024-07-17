using LightApi.SqlSugar;
using SqlSugar;

namespace LightDeploy.Server.Domain;

public class DeployHistory:ISugarTable
{
    [SugarColumn(IsPrimaryKey = true,IsIdentity=true)]
    public int Id { get; set; }
    
    /// <summary>
    /// 发布说明
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? Description { get; set; }
    
    /// <summary>
    /// 发布时间
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? PublishTime { get; set; }
    
    /// <summary>
    /// 服务id
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public int? ServiceId { get; set; }
}