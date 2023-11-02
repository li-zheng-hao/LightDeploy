using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LightDeployApp.Tables;

namespace LightDeployApp;

public class AppDataContext:INotifyPropertyChanged
{
    public List<TService> Services { get; set; }
    
    public List<TEnvironment> Environments { get; set; }
    
    public List<string> EnvironmentNames { get; set; }
    
    public List<SelectedEnvironment> SelectedEnvironments { get; set; }

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

public class SelectedEnvironment
{
    public string Name { get; set; }
    
    public string Host { get; set; }
    
    public string Port { get; set; }
    
    /// <summary>
    /// 健康检查Url
    /// </summary>
    public string? HealthCheckUrl { get; set; }

    public string Status { get; set; } = "未部署";
}

