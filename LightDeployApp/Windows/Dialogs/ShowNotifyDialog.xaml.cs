using System;
using System.Windows;

namespace LightDeployApp.Windows.Dialogs;

public partial class ShowNotifyDialog : Window
{
    public ShowNotifyDialog()
    {
        InitializeComponent();
    }
    public Action<string> Notify { get; set; }
    
    private void CancelClick(object sender, RoutedEventArgs e)
    {
       
        this.Close();
    }

    private void NotifyClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(this.NotifyContent.Text))
        {
            MessageBox.Show("请输入通知内容");
            return;            
        }
        Notify?.Invoke(this.NotifyContent.Text);
        this.Close();
    }
}