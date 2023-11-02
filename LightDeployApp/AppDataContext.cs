using System.Collections.Generic;
using LightDeployApp.Tables;

namespace LightDeployApp;

public class AppDataContext
{
    public List<TService> Services { get; set; }
    
    public List<TEnvironment> Environments { get; set; }
    
    public List<string> EnvironmentNames { get; set; }

    public List<string> DeployResult { get; set; } = new();

}

public class DeployResult
{
    public string Host { get; set; }
    
    public bool IsSuccess { get; set; }
}

