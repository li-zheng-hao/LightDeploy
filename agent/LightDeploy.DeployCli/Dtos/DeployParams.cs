using System.Collections.Generic;

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
    public string ServiceName { get; set; }
    
    /// <summary>
    /// 环境名
    /// </summary>
    public string Environment { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    public bool IsSelfContained { get; set; }

    /// <summary>
    /// 目标文件夹或者项目路径
    /// </summary>
    public string TargetPath { get; set; }
    
    public bool EnableHealthCheck { get; set; }
    
}