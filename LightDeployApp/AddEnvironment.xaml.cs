using System;
using System.Windows;
using LightDeployApp.Tables;
using MahApps.Metro.Controls;

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
        var exist=DBHelper.GetClient().Queryable<TEnvironment>().Where(it=>it.Name==environment.Name&&it.Host==environment.Host).First();
        if (exist != null)
        {
            MessageBox.Show("已存在相同环境");
            return;
        }

        DBHelper.GetClient().Insertable<TEnvironment>(environment).ExecuteCommand();
        MessageBox.Show("添加成功");
        AppContext.RefreshData();
    }


    private void ButtonBase_OnClick2(object sender, RoutedEventArgs e)
    {
        var num=DBHelper.GetClient().Deleteable<TEnvironment>()
            .Where(it => it.Name == EnvironmentName.Text)
            .ExecuteCommand();
        MessageBox.Show($"删除{num}条数据");
        AppContext.RefreshData();
    }

    private void SaveClick(object sender, RoutedEventArgs e)
    {
        try
        {
            AppContext.GetAppDataContext().Environments.ForEach(it =>
            {
                DBHelper.GetClient().Updateable(it).WhereColumns(it=>new{it.Host,it.Name}).ExecuteCommand();
            });
            MessageBox.Show("保存成功");
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message);
        }
        
    }
}