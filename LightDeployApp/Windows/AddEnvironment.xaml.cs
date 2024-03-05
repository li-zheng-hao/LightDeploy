using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using LightDeployApp.Tables;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Masuit.Tools;

namespace LightDeployApp;

public partial class AddEnvironment : MetroWindow,INotifyPropertyChanged
{
    private readonly TService _service;

    public ObservableCollection<TEnvironment> Environments { get; set; }
    public AddEnvironment(TService service)
    {
        _service = service;
        InitializeComponent();
        var environments = DBHelper.GetClient().Queryable<TEnvironment>().Where(it => it.ServiceId == _service.Id).ToList();
        Environments = new ObservableCollection<TEnvironment>();
        Environments.AddRange(environments);
        this.DataContext = this;
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        var environment = new TEnvironment
        {
            Host = Host.Text,
            Name = EnvironmentName.Text,
            Port = Port.Text,
            HealthCheckUrl = HealthCheckUrl.Text,
            AuthKey = AuthKey.Text,
            ServiceId = _service.Id
        };
        var exist=DBHelper.GetClient().Queryable<TEnvironment>().Where(it=>it.Name==environment.Name&&it.Host==environment.Host).First();
        if (exist != null)
        {
            this.ShowMessageAsync("消息",$"已存在相同环境");
            return;
        }

        DBHelper.GetClient().Insertable<TEnvironment>(environment).ExecuteCommand();
        Environments.Add(environment);
        this.ShowMessageAsync("消息",$"添加成功");

        AppContext.RefreshData();
    }


    private void ButtonBase_OnClick2(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(Id.Text,out var id))
        {
            var num=DBHelper.GetClient().Deleteable<TEnvironment>()
                .Where(it => it.Id == id)
                .ExecuteCommand();
            MessageBox.Show($"删除{num}条数据");
            AppContext.RefreshData();
        }
        else
        {
            MessageBox.Show("请输入编号数字");
        }
       
    }

    private void SaveClick(object sender, RoutedEventArgs e)
    {
        try
        {
            EditedEnvironments.ForEach(it =>
            {
                DBHelper.GetClient().Updateable(it).ExecuteCommand();
            });
            EditedEnvironments.Clear();
            MessageBox.Show("保存成功");
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message);
        }
        
    }

    private void AddEnvironment_OnLoaded(object sender, RoutedEventArgs e)
    {
        SizeToContent = SizeToContent.Manual;
    }

    private void DataGrid_OnAutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        var desc = e.PropertyDescriptor as PropertyDescriptor;
        var att = desc.Attributes[typeof(ColumnNameAttribute)] as ColumnNameAttribute;
        if(att != null)
        {
            e.Column.Header = att.Name;
        }
    }
    List<TEnvironment> EditedEnvironments=new List<TEnvironment>();
    
    private void DataGrid_OnRowEditEnding(object? sender, DataGridRowEditEndingEventArgs e)
    {
        var environment = e.Row.Item as TEnvironment;
        if (environment == null) return;
        EditedEnvironments.Add(environment);
    }

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

    private void UpGradeClick(object sender, RoutedEventArgs e)
    {
        foreach (var environment in AppContext.GetAppDataContext().Targets)
        {
            if (environment.ServiceId == null)
            {
                var service = AppContext.GetAppDataContext().Services.FirstOrDefault(it => it.Name == environment.Name);
                if (service != null)
                {
                    environment.ServiceId = service.Id;
                    DBHelper.GetClient().Updateable(environment).ExecuteCommand();
                }
            }
        }

        MessageBox.Show("初始化完成，需要重启程序");
    }
    
}