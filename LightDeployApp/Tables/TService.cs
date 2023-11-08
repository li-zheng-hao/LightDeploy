using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SqlSugar;

namespace LightDeployApp.Tables;

public class TService:INotifyPropertyChanged
{
    /// <summary>
    /// 分组名称
    /// </summary>
    public string? GroupName { get; set; }
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
    
    /// <summary>
    /// 部署完成后进行健康检查
    /// </summary>
    public bool? EnableHealthCheck { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}