using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SqlSugar;

namespace LightDeployApp.Tables;

public class TService:ViewModelBase
{
    [ColumnName("编号")]
    [SugarColumn(IsPrimaryKey = true,IsIdentity=true)]
    public int Id { get; set; }
    /// <summary>
    /// 分组名称
    /// </summary>
    [ColumnName("分组名称")]
    public string? GroupName { get; set; }
    [ColumnName("服务名称")]
    public string Name { get; set; }
    /// <summary>
    /// 0 项目 1 文件夹
    /// </summary>
    [ColumnName("部署方式")]
    public int DefaultMode { get; set; }
    
    /// <summary>
    /// 默认路径
    /// </summary>
    [ColumnName("目标路径")]
    public string DefaultTargetPath { get; set; }
    
    /// <summary>
    /// 自包含
    /// </summary>
    [ColumnName("是否包含.NET运行环境")]
    public bool IsSelfContained { get; set; }
    /// <summary>
    /// 默认环境名称
    /// </summary>
    [ColumnName("默认环境名称")]
    [SugarColumn(IsNullable = true)]
    public string? DefaultEnvironment { get; set; }
    
    /// <summary>
    /// 部署完成后进行健康检查
    /// </summary>
    [ColumnName("是否进行健康检查")]
    public bool EnableHealthCheck { get; set; }
    
    
    /// <summary>
    /// 部署完成后进行通知(企业微信)
    /// </summary>
    [ColumnName("成功后是否进行通知")]
    [SugarColumn(IsNullable = true)]
    public bool? EnableNotify { get; set; }
    
    /// <summary>
    /// 服务描述(用来在企业微信通知中显示头部)
    /// </summary>
    [ColumnName("服务描述")]
    [SugarColumn(IsNullable = true)]
    public string? Description { get; set; }
    
    /// <summary>
    /// 环境名称
    /// </summary>
    [ColumnName("环境名称")]
    [SugarColumn(IsNullable = true)]
    public string? EnvironmentName { get; set; }

}