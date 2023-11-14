using System;
using System.ComponentModel;
using System.Windows;
using Microsoft.VisualBasic;

namespace LightDeployApp.Windows;

public partial class InstallServiceParamsWindow : Window
{
    public InstallServiceParamsWindow()
    {
        InitializeComponent();
        this.DataContext = this;
    }
    public   string ServiceName { get; set; }
    public   string ServiceExe { get; set; }
    public  string ServiceParams { get; set; }
    public string ServiceDescription { get; set; }
    
    public Action<(string ServiceName,string ServiceExe,string ServiceParams,string ServiceDescription)> Confirm { get; set; }


    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        if(string.IsNullOrWhiteSpace(ServiceName)||string.IsNullOrWhiteSpace(ServiceExe)||string.IsNullOrWhiteSpace(ServiceDescription))
        {
            MessageBox.Show("服务名称,服务路径,服务描述不能为空");
            return;
        }
        
        Confirm?.Invoke((ServiceName,ServiceExe,ServiceParams,ServiceDescription));
        MessageBox.Show("保存成功");
        this.Close();
    }
}