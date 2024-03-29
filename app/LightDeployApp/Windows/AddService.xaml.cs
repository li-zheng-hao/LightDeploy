﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using LightDeployApp.Tables;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MessageBox = System.Windows.MessageBox;

namespace LightDeployApp;

public partial class AddService : MetroWindow
{
    public AddService()
    {
        InitializeComponent();
        AppContext.RefreshData();
        this.DataContext = AppContext.GetAppDataContext();
       
    }

    private void Add_OnClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Service.Text))
        {
            this.ShowMessageAsync("消息",$"请输入服务名");

            return;
        }
        var service = new TService()
        {
            GroupName=GroupName.Text,
            Name = Service.Text,
            DefaultMode = DeployMode.Text=="项目"?0:1,
            DefaultTargetPath = TargetPath.Text,
            IsSelfContained = SelfContained.IsChecked==true,
            DefaultEnvironment=Service.Text,
            EnableHealthCheck = EnableHealthCheck.IsChecked??false
        };
        var exist=DBHelper.GetClient().Queryable<TService>()
            .Any(it => it.Name == service.Name && it.EnvironmentName == service.EnvironmentName);
        if (exist)
        {
            this.ShowMessageAsync("消息", "已存在服务名、环境相同的服务");
            return;
        }
        DBHelper.GetClient().Insertable(service).ExecuteCommand();
        AppContext.RefreshData();

    }

    private void ButtonBase_OnClick2(object sender, RoutedEventArgs e)
    {
        var num=DBHelper.GetClient().Deleteable<TService>()
            .Where(it => it.Name == Service.Text)
            .ExecuteCommand();
        this.ShowMessageAsync("消息",$"删除{num}条数据");

        AppContext.RefreshData();

    }

    private void SelectDirClick(object sender, RoutedEventArgs e)
    {
        if (DeployMode.Text == "项目")
        {
            var dialog = new OpenFileDialog();
                
            dialog.Filter="project|*.csproj;";//在对话框中显示的文件类型

            dialog.ShowDialog();
            TargetPath.Text  = dialog.FileName;
               
        }
        else
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            TargetPath.Text = dialog.SelectedPath;
        }
    }

    private void SaveClick(object sender, RoutedEventArgs e)
    {
        try
        {
            EditedServices.ForEach(it =>
            {
                DBHelper.GetClient().Updateable(it).ExecuteCommand();
            });
            EditedServices.Clear();
            this.ShowMessageAsync("消息","保存成功");
        }
        catch (Exception exception)
        {
            this.ShowMessageAsync("异常",exception.Message);
        }
    }

    private void AddService_OnLoaded(object sender, RoutedEventArgs e)
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
    List<TService> EditedServices=new List<TService>();
    private void DataGrid_OnRowEditEnding(object? sender, DataGridRowEditEndingEventArgs e)
    {
        var service = e.Row.Item as TService;
        if (service == null) return;
        EditedServices.Add(service);
    }

   

    private void EditEnvironmentClick(object sender, RoutedEventArgs e)
    {
        var service = ServiceGrid.SelectedItem as TService;
        if (service == null)
        {
            this.ShowMessageAsync("错误", "未选择对应的服务");
            return;
        }

        var addEnvironment = new AddEnvironment(service);
        addEnvironment.ShowDialog();
    }
}