
using LightApi.Infra;
using LightDeploy.Domain;
using LightApi.SqlSugar;
using LightDeploy.Core.Autofac;
using LightDeployApp.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LightDeploy.Service;

public class DeployTargetService:BaseService<DeployTarget>,IScopedDependency
{
    private readonly NotifyService _notifyService;
    public DeployTargetService(NotifyService notifyService)
    {
        _notifyService = notifyService;
    }

    public async Task<string?> GetServiceStatus(DeployTarget deployTarget)
    {
        using var scope = App.ServiceProvider.CreateScope();
            
        var agentService = scope.ServiceProvider.GetRequiredService<AgentService>();
            
        await agentService.InitTargetConnection(deployTarget);

        string? status=await agentService.GetStatus(deployTarget.Service!.Name);
        
        return status;
    }
}