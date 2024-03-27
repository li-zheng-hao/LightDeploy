using LightDeploy.Core.Autofac;
using LightDeploy.Domain.Consts;
using LightDeploy.Service.Dto;
using Masuit.Tools;

namespace LightDeploy.Service;

public class DeployContext:ISingletonDependency
{
    public bool IsDeploying { get; set; } = false;

    /// <summary>
    /// 任务类型 
    /// </summary>
    public DeployTaskType TaskType { get; set; } = DeployTaskType.Deploy;
    
    public List<int>? DeployTargetIds { get; set; }
    
    public string? DeployComment { get; set; }
    
    public InstallServiceRequest? InstallServiceRequest { get; set; }
    
    public string? ZipFilePath { get; set; }

    public bool ScheduleToDeploy(List<int> targetIds,string comment,DeployTaskType deployTaskType=DeployTaskType.Deploy)
    {
        if (IsDeploying) return false;
        IsDeploying = true;
        DeployTargetIds = targetIds;
        DeployComment = comment;
        TaskType = deployTaskType;
        return true;
    }
    
  
    public bool ScheduleToInstall(List<int> targetIds,InstallServiceRequest request)
    {
        if (IsDeploying) return false;
        IsDeploying = true;
        DeployTargetIds = targetIds;
        InstallServiceRequest = request;
        TaskType = DeployTaskType.Install;
        return true;
    }
    
    public bool ScheduleToUpdateAgent(List<int> targetIds,string zipFilePath)
    {
        if (IsDeploying) return false;
        IsDeploying = true;
        DeployTargetIds = targetIds;
        TaskType = DeployTaskType.UpdateAgent;
        ZipFilePath = zipFilePath;
        return true;
    }

    public void Release()
    {
        IsDeploying = false;
        DeployTargetIds = null;
        DeployComment = null;
        InstallServiceRequest = null;
        ZipFilePath = null;
    }

    public bool NeedDeploy()
    {
        return !DeployTargetIds.IsNullOrEmpty();
    }
}

