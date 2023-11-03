using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Flurl.Http;
using MessageBox = System.Windows.MessageBox;

namespace LightDeployApp;

public partial class UpdateAgent : Window
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
                Console.WriteLine(exception);
            }
         
        }

        MessageBox.Show("发布完成,稍后所有Agent将自动更新");
    }
}