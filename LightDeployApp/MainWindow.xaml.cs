using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
        }

        private async void DeployClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("开始部署");
            
            LogBox.Text= "";
            var deployParams= new DeployParams();
            deployParams.Environment = Environment.Text;
            deployParams.ServiceName = Service.Text;
            deployParams.TargetPath = TargetPath.Text;
            deployParams.IsSelfContained = SelfContained.IsChecked==true;
            deployParams.BuildMode = DeployMode.Text == "项目" ? 0 : 1;
            await DeployService.Deploy(LogBox,deployParams);
        }

        private void AddService(object sender, RoutedEventArgs e)
        {
            var addService= new AddService();
            addService.Show();
        }

        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            RefreshData();
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
    }
}