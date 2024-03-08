using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using LightDeployApp.Tables;
using LightDeployApp.Windows;
using LightDeployApp.Windows.Dialogs;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Mapster;
using Masuit.Tools;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
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
          

        }

        private async void DeployClick(object sender, RoutedEventArgs e)
        {
            var deployParams=GetDeployParams();
        
            if(deployParams==null)return;
            
            if (deployParams.Service.Name.Contains("prod", StringComparison.OrdinalIgnoreCase))
            {
                var result=await this.ShowMessageAsync("警告","请确认是否部署到生产环境!!!!!!",MessageDialogStyle.AffirmativeAndNegative);
                if (result != MessageDialogResult.Affirmative)
                {
                    await this.ShowMessageAsync("消息", $"取消部署");
                    return ;
                }
            }
            string remark = string.Empty;
            
            if (AppContext.GetAppDataContext().Services.First(it => it.Id == deployParams.Service.Id).EnableNotify==true)
            {
                var notifyDialog = new ShowNotifyDialog();
                notifyDialog.Notify+=str =>
                {
                    remark = str;
                };
                notifyDialog.ShowDialog();
                if (string.IsNullOrWhiteSpace(remark))
                {
                    await this.ShowMessageAsync("消息",$"未输入通知内容,跳过通知");
                    deployParams.EnableNotify = false;
                }
                else
                {
                    deployParams.EnableNotify = true;
                }
            }

            if (string.IsNullOrWhiteSpace(remark))
            {
                remark=await this.ShowInputAsync("提示", "请输入此次发布说明");
            
                if (string.IsNullOrWhiteSpace(remark))
                {
                    AppContext.GetAppDataContext().Log("未输入发布说明，本次发布取消");
                    return;
                }
            }

            deployParams.Remark = remark;
          
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            AppContext.GetAppDataContext().StartDeploy();
            
            // 部署
            var deployResult=await DeployService.Deploy(deployParams);

            if (deployResult)
            {
                var history = new TDeployHistory()
                {
                    CreateTime = DateTime.Now,
                    DeployFilesDir = TargetPath.Text,
                    Remark = remark,
                    ServiceName = deployParams.Service.Name,
                    ServiceId = deployParams.Service.Id,
                    EnvironmentInfo =string.Join( ",",AppContext.GetAppDataContext().SelectedEnvironments
                        .Where(it=>it.NeedDeploy).Select(it=>$"{it.Host}"))
                };
                await DBHelper.GetClient().Insertable(history).ExecuteCommandAsync();
                if (await DBHelper.GetClient().Queryable<TDeployHistory>().Where(it=>it.ServiceId==deployParams.Service.Id).CountAsync() > 10)
                {
                    // 只保留10个
                    var histories = await DBHelper.GetClient().Queryable<TDeployHistory>().OrderByDescending(it => it.CreateTime)
                        .Take(10).ToListAsync();
                    // TODO 暂时不删除
                    // var oldestTime = histories.Min(it => it.CreateTime);
                    // await DBHelper.GetClient().Deleteable<TDeployHistory>(it=>it.CreateTime<oldestTime).ExecuteCommandAsync();
                }
                await AppContext.RefreshHistory(deployParams.Service.Id);
            }            
            
            AppContext.GetAppDataContext().StopDeploy(true);
            await this.ShowMessageAsync("消息",$"部署完成,耗时" + stopwatch.ElapsedMilliseconds + "毫秒");

        }

        private void EditService(object sender, RoutedEventArgs e)
        {
            var addService= new AddService();
            addService.Show();
            addService.Closed+= (o, args) => { RefreshData(); };
        }

           

        private void RefreshData()
        {
            AppContext.RefreshData();
            AppContext.GetAppDataContext().ServicesView.Filter = it =>
            {
                var service = it as TService;
                var data=new List<TService>(){service};
                return data.WhereIf(!string.IsNullOrWhiteSpace(ServiceGroup.SelectedValue?.ToString()),
                        it => it.GroupName == ServiceGroup.SelectedValue?.ToString())
                    .WhereIf(!string.IsNullOrWhiteSpace(Environment.SelectedValue?.ToString()), it => it.EnvironmentName == Environment.SelectedValue!.ToString())
                    .Any();
            };
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

        private  void SelectionServiceChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectService=Service.SelectedItem as TService;
            AppContext.GetAppDataContext().SelectedEnvironments?.Clear();
            if (selectService == null) return;

            DeployMode.SelectedIndex = selectService?.DefaultMode == 0 ? 0 : 1;
            TargetPath.Text = selectService?.DefaultTargetPath??"";
            SelfContained.IsChecked=selectService?.IsSelfContained;
            EnableHealthCheck.IsChecked = selectService!.EnableHealthCheck;
           
            var environments = AppContext.GetAppDataContext().Targets.Where(it => it.ServiceId == selectService!.Id).ToList();
            if (environments.Any())
            {
                AppContext.GetAppDataContext().SelectedEnvironments = new ObservableCollection<SelectedEnvironment>();
                var selectedEnvironments = environments.Adapt<List<SelectedEnvironment>>();
                    AppContext.GetAppDataContext().SelectedEnvironments.AddRange(selectedEnvironments);
            }
            
            var histories = DBHelper.GetClient().Queryable<TDeployHistory>()
                .Where(it => it.ServiceId == selectService.Id)
                .OrderByDescending(it => it.CreateTime).ToList();
            
            AppContext.GetAppDataContext().DeployHistories = histories;
            Task.Run(()=>DeployService.RefreshSelectEnvironmentsStatus(selectService.Name));

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

        private async void RefreshClick(object sender, RoutedEventArgs e)
        {
            RefreshData();
            await this.ShowMessageAsync("提示", "刷新完成");
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            SizeToContent = SizeToContent.Manual;
            DBHelper.Init();
            RefreshData();
            this.DataContext = AppContext.GetAppDataContext();
        }

        private void DataGridSelectEnvironment_OnAutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var desc = e.PropertyDescriptor as PropertyDescriptor;
            var att = desc.Attributes[typeof(ColumnNameAttribute)] as ColumnNameAttribute;
            if(att != null)
            {
                e.Column.Header = att.Name;
            }
        }

        private void StopDeployClick(object sender, RoutedEventArgs e)
        {
            AppContext.GetAppDataContext().StopDeploy();
        }

        private async void InstallServiceClick(object sender, RoutedEventArgs e)
        {
            (string ServiceName,string ServiceExe,string ServiceParams,string ServiceDescription) val = ("","","","");
            var window = new InstallServiceParamsWindow();
            window.Confirm += value =>
            {
                val = value;
            };
            window.ShowDialog();

            var deployParams = GetDeployParams();
            if (deployParams == null) return;
            try
            {
                AppContext.GetAppDataContext().StartDeploy();
                await DeployService.Install(val,deployParams);


            }
            catch (Exception exception)
            {
                AppContext.GetAppDataContext().Log(exception.Message);
                if (exception.StackTrace != null) AppContext.GetAppDataContext().Log(exception.StackTrace);
            }
            AppContext.GetAppDataContext().StopDeploy(true);

        }

        private DeployParams? GetDeployParams()
        {
            AppContext.GetAppDataContext().LogContext=string.Empty;
            var deployParams= new DeployParams
            {
                Service = Service.SelectedItem as TService,
                TargetPath = TargetPath.Text,
                IsSelfContained = SelfContained.IsChecked==true,
                EnableHealthCheck = EnableHealthCheck.IsChecked==true,
                BuildMode = DeployMode.Text == "项目" ? 0 : 1
            };

            if (deployParams.Service==null)
            {
                this.ShowMessageAsync("消息",$"请选择服务");

                return null;
            }

            if (string.IsNullOrWhiteSpace(deployParams.TargetPath))
            {
                this.ShowMessageAsync("消息",$"请选择目标路径");

                return null;
            }

            return deployParams;
        }

        private async void StartServiceClick(object sender, RoutedEventArgs e)
        {
            var deployParams = GetDeployParams();
            if(deployParams==null) return;
            AppContext.Log("开始启动服务");
            await DeployService.StartService(deployParams);
            await this.ShowMessageAsync("消息",$"启动完成");
        }

        private async void StopServiceClick(object sender, RoutedEventArgs e)
        {
            var deployParams = GetDeployParams();
            if(deployParams==null) return;
            await DeployService.StopService(deployParams);
            await this.ShowMessageAsync("消息",$"停止完成");
        }

        private void SelectionServiceGroupChanged(object sender, SelectionChangedEventArgs e)
        {
            AppContext.GetAppDataContext().ServicesView.Refresh();
        }


        private void DevelopEnvironment_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppContext.GetAppDataContext().ServicesView.Refresh();
        }
    }
}