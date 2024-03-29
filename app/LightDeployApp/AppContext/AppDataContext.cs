﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Data;
using LightDeployApp.Tables;

namespace LightDeployApp;

public class AppDataContext : ViewModelBase
{
    public object locker = new();

    public string UniqueId { get; set; } = Guid.NewGuid().ToString();

    public TGlobalSetting GlobalSetting { get; set; }
    public List<TService> Services { get; set; }

    public List<TDeployHistory> DeployHistories { get; set; }
    public List<TEnvironment> Targets { get; set; }

    public List<string> EnvironmentNames { get; set; }

    public ObservableCollection<SelectedEnvironment>? SelectedEnvironments { get; set; }

    /// <summary>
    /// 服务
    /// </summary>
    public ICollectionView ServicesView { get; set; }
    
    public List<string> ServiceGroupNames { get; set; }
    
    /// <summary>
    /// 环境  测试 开发 预览 生产等
    /// </summary>
    public List<string> Environments { get; set; }
    
    public string LogContext { get; set; }

    /// <summary>
    /// 取消部署Token
    /// </summary>
    public CancellationTokenSource? StopToken { get; set; }

    /// <summary>
    /// 是否处于空闲状态
    /// </summary>
    public bool IsIdle { get; set; } = true;
    
    public void Log(string? data)
    {
        if(string.IsNullOrWhiteSpace(data)) return;
        lock (locker)
        {
            LogContext += data + "\n";
        }
    }

    public void StartDeploy()
    {
        LogContext = string.Empty;
        StopToken = new CancellationTokenSource();
        IsIdle = false;
    }

    public void StopDeploy(bool isSuccess=false)
    {
        StopToken?.Cancel();
        if(!isSuccess)
            Log("已取消部署");
        StopToken = null;
        IsIdle = true;
    }
}

public class SelectedEnvironment : TEnvironment
{
    [ColumnName("处理状态")] public string Status { get; set; } = "未部署";

    [ColumnName("是否需要部署")] public bool NeedDeploy { get; set; } = true;
    
    [ColumnName("服务运行状态")]public string ServiceStatus { get; set; }
}