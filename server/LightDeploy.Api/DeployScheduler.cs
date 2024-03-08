using LightApi.Infra;
using LightApi.Infra.Extension;
using LightDeploy.Domain.Consts;
using LightDeploy.Service;
using Serilog;

namespace LightDeploy.Api;

public class DeployScheduler:BackgroundService
{
    private readonly NotifyService _notifyService;
    private readonly DeployContext _deployContext;

    public DeployScheduler(NotifyService notifyService,DeployContext deployContext)
    {
        _notifyService = notifyService;
        _deployContext = deployContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                if (_deployContext.NeedDeploy())
                {
                    if(_deployContext.TaskType==DeployTaskType.Install)
                        await Install();
                    else if(_deployContext.TaskType==DeployTaskType.Deploy)
                        await Deploy();
                    else if(_deployContext.TaskType==DeployTaskType.UpdateAgent)
                    {
                        await UpdateAgent();
                    }
                    else
                    {
                        Log.Warning("未知的任务类型");
                    }
                    
                    _deployContext.Release();

                }
                else
                {
                    Log.Debug("无任务执行，休息");
                    await Task.Delay(2000, stoppingToken);
                }
            }
        }
        catch (TaskCanceledException)
        {
            // ignore
        }
        catch (Exception e)
        {
            Log.Error(e, e.Message);
        }
       
    }

    /// <summary>
    /// 更新客户端
    /// </summary>
    private async Task UpdateAgent()
    {
        using var scope=App.ServiceProvider.CreateScope();
        var deployTargetService = scope.ServiceProvider.GetService<DeployTargetService>();
        var operationService = scope.ServiceProvider.GetService<OperationService>();
        var targets = await deployTargetService.Repository.AsQueryable()
            .Where(it => _deployContext.DeployTargetIds!.Contains(it.Id))
            .Includes(it => it.Service)
            .ToListAsync();
        Check.ThrowIf(targets.Select(it => it.ServiceId).Distinct().Count() > 1, "只能选择同一服务的发布目标");
        await operationService.UpdateAgent(targets,_deployContext.ZipFilePath);
    }

    private async Task Install()
    {
        using var scope=App.ServiceProvider.CreateScope();
        var deployTargetService = scope.ServiceProvider.GetService<DeployTargetService>();
        var operationService = scope.ServiceProvider.GetService<OperationService>();
        var targets = await deployTargetService.Repository.AsQueryable()
            .Where(it => _deployContext.DeployTargetIds!.Contains(it.Id))
            .Includes(it => it.Service)
            .ToListAsync();
        Check.ThrowIf(targets.Select(it => it.ServiceId).Distinct().Count() > 1, "只能选择同一服务的发布目标");
        await operationService.Install(targets,_deployContext.InstallServiceRequest);
    }

    private async Task Deploy()
    {
        using var scope=App.ServiceProvider.CreateScope();
        var deployTargetService = scope.ServiceProvider.GetService<DeployTargetService>();
        var operationService = scope.ServiceProvider.GetService<OperationService>();
        var targets = await deployTargetService.Repository.AsQueryable()
            .Where(it => _deployContext.DeployTargetIds!.Contains(it.Id))
            .Includes(it => it.Service)
            .ToListAsync();
        Check.ThrowIf(targets.Select(it => it.ServiceId).Distinct().Count() > 1, "只能选择同一服务的发布目标");
        await operationService.Deploy(targets,_deployContext.DeployComment);
    }

  
}