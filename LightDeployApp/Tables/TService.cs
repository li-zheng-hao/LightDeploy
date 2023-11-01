using System;
using SqlSugar;

namespace LightDeployApp.Tables;

public class TService
{
    public string Name { get; set; }
    /// <summary>
    /// 0 项目 1 文件夹
    /// </summary>
    public int DefaultMode { get; set; }
    /// <summary>
    /// 默认路径
    /// </summary>
    public string DefaultTargetPath { get; set; }
    
    /// <summary>
    /// 自包含
    /// </summary>
    public bool IsSelfContained { get; set; }
    /// <summary>
    /// 默认环境名称
    /// </summary>
    public string DefaultEnvironment { get; set; }
}