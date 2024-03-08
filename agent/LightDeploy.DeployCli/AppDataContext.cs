using LightDeploy.DeployCli.Tables;

namespace LightDeployApp;

public class AppDataContext
{
    public List<TService> Services { get; set; }
    
    public List<TEnvironment> Environments { get; set; }
    
}

