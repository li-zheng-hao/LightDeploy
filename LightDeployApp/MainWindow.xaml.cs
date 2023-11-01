using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using LightDeployApp.Tables;
using MessageBox = System.Windows.MessageBox;

namespace LightDeployApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<TService> _services=new();
        private List<TEnvironment> _environments=new();
        private string projectPath = "";
        public MainWindow()
        {
            InitializeComponent();
            DBHelper.Init();
            RefreshData();
        }

        private void AddEnvironmentClick(object sender, RoutedEventArgs e)
        {
            var addEnvironment = new AddEnvironment();
            addEnvironment.Show();
            addEnvironment.Closed+= (o, args) => { RefreshData(); };
        }

        private async void DeployClick(object sender, RoutedEventArgs e)
        {
            LogBox.Text= "";
            var deployParams= new DeployParams();
            deployParams.Environment = Environment.Text;
            deployParams.ServiceName = Service.Text;
            deployParams.TargetPath = TargetPath.Text;
            deployParams.IsSelfContained = SelfContained.IsChecked==true;
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
            await DeployService.Deploy(LogBox,deployParams);
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
            _services = DBHelper.GetClient()
                .Queryable<TService>()
                .ToList();
            
            _environments= DBHelper.GetClient()
                .Queryable<TEnvironment>()
                .ToList();
            
            Service.ItemsSource = _services.Select(it=>it.Name);
            Environment.ItemsSource = _environments.Select(it=>it.Name).Distinct();
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
            var selectService=_services.FirstOrDefault(it => it.Name == Service.SelectedValue.ToString());
            DeployMode.SelectedIndex = selectService?.DefaultMode == 0 ? 0 : 1;
            TargetPath.Text = selectService?.DefaultTargetPath;
            SelfContained.IsChecked=selectService?.IsSelfContained;
            var environment = _environments.FirstOrDefault(it => it.Name == selectService!.DefaultEnvironment);
            if(environment!=null)
                Environment.SelectedIndex = _environments.Select(it=>it.Name).Distinct().ToList().IndexOf(environment.Name);
        }

        private void LogBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            LogBox.ScrollToEnd();
        }
    }
}