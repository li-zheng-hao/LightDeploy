using System.Collections.Generic;
using LightDeployApp.Tables;

namespace LightDeployApp;

public class DeployParams
{
    /// <summary>
    /// 编译方式 0项目 1文件夹
    /// </summary>
    public int BuildMode { get; set; }
    
    /// <summary>
    /// 服务名
    /// </summary>
    public TService Service { get; set; }
    
    /// <summary>
    /// 需要发布的环境
    /// </summary>
    public List<TEnvironment> Environments { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    public bool IsSelfContained { get; set; }

    /// <summary>
    /// 目标文件夹或者项目路径
    /// </summary>
    public string TargetPath { get; set; }
    
    public bool EnableHealthCheck { get; set; }
    
    public bool EnableNotify { get; set; }
    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; set; }
}