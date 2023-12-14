using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Flurl.Http;
using Flurl.Http.Configuration;
using LightDeployApp.Dtos;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MessageBox = System.Windows.MessageBox;

namespace LightDeployApp;

public partial class UpdateAgent : MetroWindow
{
    public UpdateAgent()
    {
        InitializeComponent();
        this.DataContext = AppContext.GetAppDataContext().Environments.DistinctBy(it=>it.Host)
            .Select(it=>new UpdateDeployDto()
            {
                Name = it.Name,
                Port = it.Port,
                Host = it.Host,
                NeedDeploy = true,
                AuthKey=it.AuthKey
            }).ToList();
    }

    private void SelectDirClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog();
                
        dialog.Filter="压缩包|*.zip;*.rar";//在对话框中显示的文件类型

        dialog.ShowDialog();
        
        if (!string.IsNullOrWhiteSpace(dialog.FileName))
        {
            FilePath.Text = dialog.FileName;
        }
    }

    private async void PublishUpdate(object sender, RoutedEventArgs e)
    {
        await this.ShowMessageAsync("消息",$"开始发布");

        var data=this.DataContext as List<UpdateDeployDto> ?? throw new InvalidOperationException();
        foreach (var environment in data.Where(it=>it.NeedDeploy))
        {
            LogBox.Text+="开始发布:"+environment.Host+ "\n";

            try
            {
                var url = $"http://{environment.Host}:{environment.Port}/api/deploy/updateself";
                await url
                    .WithHeader("Authorization",environment.AuthKey)
                    .PostMultipartAsync(content =>
                {
                    content.AddFile("file", FilePath.Text);
                });
            }
            catch (FlurlHttpException ex)
            {
                var body=await ex.GetResponseStringAsync();
                LogBox.Text+=$"部署失败{environment.Host}:{environment.Port}";
                LogBox.Text+=$"返回消息 {ex.Message}";
                LogBox.Text+=$"返回消息 {body}";
                
            }
            
            LogBox.Text+="发布完成:"+environment.Host+ "\n";

        }
        this.ShowMessageAsync("消息","发布完成,稍后所有Agent将自动更新");

    }

    private void LogBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        LogBox.ScrollToEnd();
    }

    private void DataGridSelectEnvironment_OnAutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        var desc = e.PropertyDescriptor as PropertyDescriptor;
        var att = desc.Attributes[typeof(ColumnNameAttribute)] as ColumnNameAttribute;
        if(att != null)
        {
            e.Column.Header = att.Name;
        }
    }
}