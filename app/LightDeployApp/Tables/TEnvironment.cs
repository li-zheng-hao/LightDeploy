using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SqlSugar;

namespace LightDeployApp.Tables;

/// <summary>
/// 发布环境
/// </summary>
public class TEnvironment:ViewModelBase
{
    [ColumnName("编号")]
    [SugarColumn(IsPrimaryKey = true,IsIdentity=true)]
    public int Id { get; set; }
    
    [ColumnName("服务名称")]
    public string Name { get; set; }
    
    [ColumnName("主机")]
    public string Host { get; set; }
    [ColumnName("端口")]
    public string Port { get; set; }
    
    /// <summary>
    /// 健康检查Url
    /// </summary>
    [ColumnName("健康检查URL")]
    [SugarColumn(IsNullable = true)]
    public string? HealthCheckUrl { get; set; }
    
    /// <summary>
    /// Token
    /// </summary>
    [ColumnName("认证Token")]
    [SugarColumn(IsNullable = true)]
    public string? AuthKey { get; set; }
    
    /// <summary>
    /// 服务Id
    /// </summary>
    [ColumnName("服务ID")]
    [SugarColumn(IsNullable = true)]
    public int? ServiceId { get; set; }

    
}