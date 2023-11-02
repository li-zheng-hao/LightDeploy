using System.Windows;
using System.Windows.Forms;
using LightDeployApp.Tables;
using MessageBox = System.Windows.MessageBox;

namespace LightDeployApp;

public partial class AddService : Window
{
    public AddService()
    {
        InitializeComponent();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Service.Text))
        {
            MessageBox.Show("请输入服务名");
            return;
        }
        var service = new TService()
        {
            Name = Service.Text,
            DefaultMode = DeployMode.Text=="项目"?0:1,
            DefaultTargetPath = TargetPath.Text,
            IsSelfContained = SelfContained.IsChecked==true,
            DefaultEnvironment=Service.Text,
            EnableHealthCheck = EnableHealthCheck.IsChecked
        };
       
        DBHelper.GetClient().Deleteable<TService>(it=>it.Name==service.Name).ExecuteCommand();

        DBHelper.GetClient().Insertable(service).ExecuteCommand();
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
}