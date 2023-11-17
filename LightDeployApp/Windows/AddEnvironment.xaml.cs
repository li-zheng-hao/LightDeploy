using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using LightDeployApp.Tables;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace LightDeployApp;

public partial class AddEnvironment : MetroWindow
{
    public AddEnvironment()
    {
        InitializeComponent();
        this.DataContext = AppContext.GetAppDataContext();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        var environment = new TEnvironment();
        environment.Host=Host.Text;
        environment.Name=EnvironmentName.Text;
        environment.Port=Port.Text;
        environment.HealthCheckUrl = HealthCheckUrl.Text;
        environment.AuthKey = AuthKey.Text;
        var exist=DBHelper.GetClient().Queryable<TEnvironment>().Where(it=>it.Name==environment.Name&&it.Host==environment.Host).First();
        if (exist != null)
        {
            this.ShowMessageAsync("消息",$"已存在相同环境");

            return;
        }

        DBHelper.GetClient().Insertable<TEnvironment>(environment).ExecuteCommand();
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
}