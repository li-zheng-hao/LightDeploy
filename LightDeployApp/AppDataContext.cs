﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using LightDeployApp.Tables;

namespace LightDeployApp;

public class AppDataContext:INotifyPropertyChanged
{
    public object locker = new();
    public string UniqueId { get; set; }=Guid.NewGuid().ToString();
    
    public List<TService> Services { get; set; }
    
    public List<TEnvironment> Environments { get; set; }
    
    public List<string> EnvironmentNames { get; set; }
    
    public List<SelectedEnvironment> SelectedEnvironments { get; set; }
    
    public ListCollectionView ServicesView { get; set; }
    
    public string LogContext { get; set; }
    
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

    public void Log(string data)
    {
        lock (locker)
        {
            LogContext+=data+"\n";
        }
    }
}

public class SelectedEnvironment:TEnvironment
{
    [ColumnName("处理状态")]
    public string Status { get; set; } = "未部署";

    [ColumnName("是否需要部署")]
    public bool NeedDeploy { get; set; } = true;
}

