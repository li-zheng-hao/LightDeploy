using LightApi.Infra.Extension;
using LightDeploy.Domain;
using LightDeploy.Service;
using LightDeploy.Service.Dto;
using Masuit.Tools;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace LightDeploy.Api.Controllers;

/// <summary>
/// 发布服务
/// </summary>
[ApiController]
[Route("api/deploy")]
public class DeployController : ControllerBase
{
    private readonly OperationService _operationService;
    private readonly DeployTargetService _deployTargetService;

    public DeployController(OperationService operationService, DeployTargetService deployTargetService)
    {
        _operationService = operationService;
        _deployTargetService = deployTargetService;
    }

    [HttpPost("send")]
    public async Task<IActionResult> Test([FromServices] NotifyService notifyService)
    {
        await notifyService.NotifyMessageToUser("测试发送");
        return Ok();
    }

    /// <summary>
    /// 部署服务 发布更新
    /// </summary>
    /// <param name="targetIds"></param>
    /// <param name="deployComment">发布说明</param>
    /// <returns></returns>
    [HttpPost("deploy")]
    public IActionResult Deploy([FromQuery] List<int> targetIds, [FromQuery] string deployComment,[FromServices]DeployContext deployContext)
    {
        Check.NotNullOrEmpty(targetIds, "发布目标不能为空");
        Check.ThrowIf(deployContext.IsDeploying,"正在执行操作中，请稍后再试");
        deployContext.ScheduleToDeploy(targetIds, deployComment);
        return Ok();
    }

    /// <summary>
    /// 安装服务
    /// </summary>
    /// <param name="targetIds"></param>
    /// <returns></returns>
    [HttpPost("install")]
    public IActionResult Install([FromQuery] List<int> targetIds,[FromBody]InstallServiceRequest request,[FromServices]DeployContext deployContext)
    {
        Log.Information($"安装参数{request.ToJsonString()}");
        Check.NotNullOrEmpty(targetIds, "发布目标不能为空");
        Check.ThrowIf(deployContext.IsDeploying,"正在执行操作中，请稍后再试");
        deployContext.ScheduleToInstall(targetIds,request);
        return Ok();
    }

    /// <summary>
    /// 启动服务
    /// </summary>
    /// <param name="targetIds"></param>
    /// <returns></returns>
    [HttpPost("start-service")]
    public async Task StartService([FromQuery] List<int> targetIds)
    {
        Check.NotNullOrEmpty(targetIds, "发布目标不能为空");
        var targets = await _deployTargetService.Repository.AsQueryable().Where(it => targetIds.Contains(it.Id))
            .Includes(it => it.Service).ToListAsync();
        foreach (var deployTarget in targets)
        {
            await _operationService.StartService(deployTarget);
        }
    }

    /// <summary>
    /// 停止服务
    /// </summary>
    /// <param name="targetIds"></param>
    /// <returns></returns>
    [HttpPost("stop-service")]
    public async Task StopService([FromQuery] List<int> targetIds)
    {
        Check.NotNullOrEmpty(targetIds, "发布目标不能为空");
        var targets = await _deployTargetService.Repository.AsQueryable().Where(it => targetIds.Contains(it.Id))
            .Includes(it => it.Service).ToListAsync();
        foreach (var deployTarget in targets)
        {
            await _operationService.StopService(deployTarget);
        }
    }
    /// <summary>
    /// 更新客户端代理
    /// </summary>
    /// <param name="targetIds"></param>
    [HttpPost("update-agent")]
    public IActionResult UpdateAgent([FromQuery] List<int> targetIds,[FromQuery] string zipFilePath,[FromServices]DeployContext deployContext)
    {
        Check.NotNullOrEmpty(targetIds, "发布目标不能为空");
        Check.ThrowIf(!System.IO.File.Exists(zipFilePath), "文件不存在");
        Check.ThrowIf(!zipFilePath.EndsWith(".zip")&&!zipFilePath.EndsWith(".rar"), "文件格式不正确,必须为rar或zip格式");
        Check.ThrowIf(deployContext.IsDeploying,"正在执行操作中，请稍后再试");
        deployContext.ScheduleToUpdateAgent(targetIds,zipFilePath);
        return Ok();
    }
}