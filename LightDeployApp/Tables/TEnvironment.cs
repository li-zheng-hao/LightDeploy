using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LightDeployApp.Tables;

public class TEnvironment:ViewModelBase
{
    [ColumnName("环境名称")]
    public string Name { get; set; }
    [ColumnName("主机")]
    public string Host { get; set; }
    [ColumnName("端口")]
    public string Port { get; set; }
    
    /// <summary>
    /// 健康检查Url
    /// </summary>
    [ColumnName("健康检查URL")]
    public string? HealthCheckUrl { get; set; }
    
    /// <summary>
    /// Token
    /// </summary>
    [ColumnName("认证Token")]
    public string? AuthKey { get; set; }

    
}