using LightApi.SqlSugar;
using SqlSugar;

namespace LightDeploy.Server.Domain;

public class DeployTarget:ISugarTable
{
    [SugarColumn(IsPrimaryKey = true,IsIdentity=true)]
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
    [SugarColumn(IsNullable = true)]
    public string? HealthCheckUrl { get; set; }
    
    /// <summary>
    /// Token
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? AuthKey { get; set; }
    
    public int? ServiceId { get; set; }
    
    [Navigate(NavigateType.OneToOne,nameof(ServiceId),nameof(DeployService.Id))]
    public DeployService? Service { get; set; }
}

