using System;
using System.Windows;
using System.Windows.Forms;
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
        this.DataContext = AppContext.GetAppDataContext();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Service.Text))
        {
            this.ShowMessageAsync("消息",$"请输入服务名");

            return;
        }
        var service = new TService()
        {
            Name = Service.Text,
            DefaultMode = DeployMode.Text=="项目"?0:1,
            DefaultTargetPath = TargetPath.Text,
            IsSelfContained = SelfContained.IsChecked==true,
            DefaultEnvironment=Environment.Text,
            EnableHealthCheck = EnableHealthCheck.IsChecked
        };
       
        DBHelper.GetClient().Deleteable<TService>(it=>it.Name==service.Name).ExecuteCommand();

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
            AppContext.GetAppDataContext().Services.ForEach(it =>
            {
                DBHelper.GetClient().Updateable(it).WhereColumns(it=>new{it.Name}).ExecuteCommand();
            });
            this.ShowMessageAsync("消息","保存成功");
        }
        catch (Exception exception)
        {
            this.ShowMessageAsync("异常",exception.Message);
        }
    }
}