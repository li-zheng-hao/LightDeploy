using System.Windows;
using LightDeployApp.Tables;

namespace LightDeployApp;

public partial class AddService : Window
{
    public AddService()
    {
        InitializeComponent();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        var service = new TService()
        {
            Name = Service.Text
        };
        var exist=DBHelper.GetClient().Queryable<TService>()
            .First(it => it.Name == service.Name);
        if (exist != null)
        {
            MessageBox.Show("已存在同名服务");
            return;
        }

        DBHelper.GetClient().Insertable<TService>(service).ExecuteCommand();
        Close();
    }

    private void ButtonBase_OnClick2(object sender, RoutedEventArgs e)
    {
        var num=DBHelper.GetClient().Deleteable<TService>()
            .Where(it => it.Name == Service.Text)
            .ExecuteCommand();
        MessageBox.Show($"删除{num}条数据");
        Close();
    }
}