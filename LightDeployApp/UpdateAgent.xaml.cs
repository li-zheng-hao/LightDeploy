using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Flurl.Http;
using MahApps.Metro.Controls;
using MessageBox = System.Windows.MessageBox;

namespace LightDeployApp;

public partial class UpdateAgent : MetroWindow
{
    public UpdateAgent()
    {
        InitializeComponent();
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
        MessageBox.Show("开始发布");

        foreach (var environment in AppContext.GetAppDataContext().Environments)
        {
            LogBox.Text+="开始发布:"+environment.Host+ "\n";

            try
            {
                var url = $"http://{environment.Host}:{environment.Port}/api/deploy/updateself";
                await url.PostMultipartAsync(content =>
                {
                    content.AddFile("file", FilePath.Text);
                });
            }
            catch (Exception exception)
            {
                LogBox.Text+="发布失败:"+environment.Host+ $",异常：{exception.Message} \n";
                Console.WriteLine(exception);
            }
            
            LogBox.Text+="发布完成:"+environment.Host+ "\n";

        }

        MessageBox.Show("发布完成,稍后所有Agent将自动更新");
    }

    private void LogBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        LogBox.ScrollToEnd();
    }
}