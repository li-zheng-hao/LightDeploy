using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using LightDeployApp.Tables;
using MahApps.Metro.Controls;
using Microsoft.Extensions.DependencyInjection;
using MessageBox = System.Windows.MessageBox;

namespace LightDeployApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private string projectPath = "";
        public MainWindow()
        {
            InitializeComponent();
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<AppDataContext>();
            AppContext.ServiceProvider=serviceCollection.BuildServiceProvider();
            DBHelper.Init();
            RefreshData();
            this.DataContext = AppContext.GetAppDataContext();

        }

        private void AddEnvironmentClick(object sender, RoutedEventArgs e)
        {
            var addEnvironment = new AddEnvironment();
            addEnvironment.Show();
            addEnvironment.Closed+= (o, args) => { RefreshData(); };
        }

        private async void DeployClick(object sender, RoutedEventArgs e)
        {
            AppContext.GetAppDataContext().LogContext=string.Empty;
            var deployParams= new DeployParams();
            deployParams.Environment = Environment.Text;
            deployParams.ServiceName = Service.Text;
            deployParams.TargetPath = TargetPath.Text;
            deployParams.IsSelfContained = SelfContained.IsChecked==true;
            deployParams.EnableHealthCheck = EnableHealthCheck.IsChecked==true;
            deployParams.BuildMode = DeployMode.Text == "项目" ? 0 : 1;
            
            if(string.IsNullOrWhiteSpace(deployParams.Environment))
            {
                MessageBox.Show("请选择环境");
                return;
            }

            if (string.IsNullOrWhiteSpace(deployParams.ServiceName))
            {
                MessageBox.Show("请选择服务");
                return;
            }

            if (string.IsNullOrWhiteSpace(deployParams.TargetPath))
            {
                MessageBox.Show("请选择目标路径");
                return;
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            await DeployService.Deploy(deployParams);
            MessageBox.Show("部署完成,耗时" + stopwatch.ElapsedMilliseconds + "ms");
        }

        private void AddService(object sender, RoutedEventArgs e)
        {
            var addService= new AddService();
            addService.Show();
            addService.Closed+= (o, args) => { RefreshData(); };
        }

           

        private void RefreshData()
        {
            AppContext.RefreshData();
        }

        private void SelectDirClick(object sender, RoutedEventArgs e)
        {
            if (DeployMode.Text == "项目")
            {
                var dialog = new OpenFileDialog();
                
                dialog.Filter="project|*.csproj;";//在对话框中显示的文件类型

                dialog.ShowDialog();
                projectPath = dialog.FileName;
                TargetPath.Text = projectPath;
               
            }
            else
            {
                var dialog = new FolderBrowserDialog();
                dialog.ShowDialog();
                projectPath = dialog.SelectedPath;
                TargetPath.Text = projectPath;
            }
          
        }

        private void SelectionServiceChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Service.SelectedValue == null) return;
            var selectService=AppContext.GetAppDataContext().Services.FirstOrDefault(it => it.Name == Service.SelectedValue.ToString());
            if (selectService == null) return;

            DeployMode.SelectedIndex = selectService?.DefaultMode == 0 ? 0 : 1;
            TargetPath.Text = selectService?.DefaultTargetPath;
            SelfContained.IsChecked=selectService?.IsSelfContained;
            EnableHealthCheck.IsChecked = selectService.EnableHealthCheck;
            var environments = AppContext.GetAppDataContext().Environments.Where(it => it.Name == selectService!.DefaultEnvironment).ToList();
            if (environments != null&&environments.Any())
            {
                Environment.SelectedIndex = AppContext.GetAppDataContext().Environments.Select(it=>it.Name).Distinct().ToList().IndexOf(environments.First().Name);
                var data=environments.Select(it =>
                    new SelectedEnvironment()
                    {
                        Name = it.Name,
                        Host = it.Host,
                        Port = it.Port,
                        HealthCheckUrl = it.HealthCheckUrl
                    }).ToList();
                AppContext.GetAppDataContext().SelectedEnvironments=
                   data;
            }
            
        }

        private void LogBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            LogBox.ScrollToEnd();
        }

        private void UpdateAgent(object sender, RoutedEventArgs e)
        {
            var updateAgent = new UpdateAgent();
            updateAgent.Show();
        }

        private void Environment_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           var currents=AppContext.GetAppDataContext().Environments.Where(it => it.Name == Environment.Text).ToList();
            var data= currents.Select(it =>
                new SelectedEnvironment()
                {
                    Name = it.Name,
                    Host = it.Host,
                    Port = it.Port,
                    HealthCheckUrl = it.HealthCheckUrl
                }).ToList();
            AppContext.GetAppDataContext().SelectedEnvironments=
                data;
        }
       
    }
}