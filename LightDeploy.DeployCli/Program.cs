// See https://aka.ms/new-console-template for more information

using CommandLine;
using LightDeploy.DeployCli;
using LightDeployApp;
using Masuit.Tools;
using AppContext = LightDeployApp.AppContext;

var parserResult = Parser.Default.ParseArguments<CommandParameters>(args);
if (!parserResult.Errors.IsNullOrEmpty())
    Environment.Exit(-1);

var parameters = parserResult.Value;

if (parameters.Operation == "publish")
{
    var dataContext = AppContext.GetAppDataContext();
    
    var environments = dataContext.Environments.Where(it=>it.Name==parameters.Environment).ToList();
    var param = new DeployParams()
    {
        ServiceName = parameters.Service,
        Environment = parameters.Environment,
        TargetPath = parameters.TargetProjectPath,
    };
    await DeployService.Deploy(param);
}
    