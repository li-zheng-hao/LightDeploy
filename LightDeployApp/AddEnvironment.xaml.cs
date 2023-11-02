using System.Windows;
using LightDeployApp.Tables;

namespace LightDeployApp;

public partial class AddEnvironment : Window
{
    public AddEnvironment()
    {
        InitializeComponent();
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
        Close();
    }


    private void ButtonBase_OnClick2(object sender, RoutedEventArgs e)
    {
        var num=DBHelper.GetClient().Deleteable<TEnvironment>()
            .Where(it => it.Name == EnvironmentName.Text)
            .ExecuteCommand();
        MessageBox.Show($"删除{num}条数据");
        Close();
    }
}