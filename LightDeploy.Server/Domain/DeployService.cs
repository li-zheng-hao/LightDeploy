﻿using System.ComponentModel.DataAnnotations;
using LightApi.SqlSugar;
using SqlSugar;

namespace LightDeploy.Server.Domain;

public class DeployService:ISugarTable
{
    [SugarColumn(IsPrimaryKey = true,IsIdentity=true)]
    public int Id { get; set; }
    
    /// <summary>
    /// 分组名称
    /// </summary>
    [Required(ErrorMessage = "分组不能为空")]
    [Length(minimumLength:1,maximumLength:255)]
    public string? GroupName { get; set; }
    
    /// <summary>
    /// 服务名称
    /// </summary>
    [Required(ErrorMessage = "服务名不能为空")]
    [Length(minimumLength:1,maximumLength:255)]
    public string Name { get; set; }
    
    /// <summary>
    /// 0 项目 1 文件夹
    /// </summary>
    public int DeployMode { get; set; }
    
    /// <summary>
    /// 默认路径
    /// </summary>
    [Required(ErrorMessage = "项目路径不能为空")]
    [Length(minimumLength:1,maximumLength:255)]
    public string ProjectPath { get; set; }
    
    /// <summary>
    /// 自包含
    /// </summary>
    public bool IsSelfContained { get; set; }
    
    /// <summary>
    /// 部署完成后进行健康检查
    /// </summary>
    public bool EnableHealthCheck { get; set; }
    
    
    /// <summary>
    /// 部署完成后进行通知(企业微信)
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public bool? EnableNotify { get; set; }
    
    /// <summary>
    /// 服务描述(用来在企业微信通知中显示头部)
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? Description { get; set; }
    
    /// <summary>
    /// 环境名称
    /// </summary>
    [SugarColumn(IsNullable = true)]
    [Required(ErrorMessage = "环境不能为空")]
    public string? EnvironmentName { get; set; }
    
    [Navigate(NavigateType.OneToMany, nameof(DeployTarget.ServiceId),nameof(Id))] 
    public List<DeployTarget>? Targets { get; set; }//只能是null不能赋默认值
    
    
    /// <summary>
    /// 服务端口
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public int? Port { get; set; }
    
    /// <summary>
    /// 忽略规则 正则表达式 用|分隔多个规则
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? IgnoreRules { get; set; }
    
    /// <summary>
    /// 发布目标文件夹 不填则通过服务名找目标文件夹
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? TargetDir { get; set; }
    
    /// <summary>
    /// 是否仅复制文件，不启动和停止服务
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public bool? OnlyCopyFiles { get; set; }
    
    /// <summary>
    /// 提前运行的powershell脚本 
    /// </summary>
    [SugarColumn(IsNullable = true,ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? BeforeScript { get; set; }
}