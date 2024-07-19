using System.ComponentModel.DataAnnotations;

namespace LightDeploy.Server.Dtos;

public class DeployServiceDto
{
    public int Id { get; set; }
    
    /// <summary>
    /// 分组名称
    /// </summary>
    [Display(Name = "分组")]
    public string? GroupName { get; set; }
    
    /// <summary>
    /// 服务名称
    /// </summary>
    [Display(Name = "服务名称")]
    public string Name { get; set; }
    
    /// <summary>
    /// 0 项目 1 文件夹
    /// </summary>
    public int DeployMode { get; set; }
    
    /// <summary>
    /// 默认路径
    /// </summary>
    [Display(Name = "目标路径")]
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
    public bool? EnableNotify { get; set; }
    
    /// <summary>
    /// 服务描述(用来在企业微信通知中显示头部)
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 环境名称
    /// </summary>
    [Display(Name = "环境名称")]
    public string? EnvironmentName { get; set; }
    
    /// <summary>
    /// 服务端口
    /// </summary>
    [Display(Name = "服务端口")]
    public int? Port { get; set; }
    
    /// <summary>
    /// 是否只拷贝文件，不启动和停止服务
    /// </summary>
    [Display(Name = "是否只拷贝文件")]
    public bool? OnlyCopyFile { get; set; }
    
    /// <summary>
    /// 发布目标文件夹 不填则通过服务名找目标文件夹
    /// </summary>
    public string? TargetDir { get; set; }
}