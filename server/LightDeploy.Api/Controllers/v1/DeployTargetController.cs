using LightApi.Infra.Extension;
using LightDeploy.Domain;
using LightDeploy.Service;
using LightDeploy.Service.Dto;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace LightDeploy.Api.Controllers;

/// <summary>
/// 目标
/// </summary>
[ApiController]
[Route("api/target")]
public class DeployTargetController : ControllerBase
{

    private readonly ILogger<DeployServiceController> _logger;
    private readonly DeployTargetService _deployTargetService;

    public DeployTargetController(ILogger<DeployServiceController> logger,DeployTargetService deployTargetService)
    {
        _logger = logger;
        _deployTargetService = deployTargetService;
    }

    /// <summary>
    /// 获取服务状态
    /// </summary>
    /// <param name="targetId"></param>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus([FromQuery] int targetId)
    {
        var deployTarget =await _deployTargetService.Repository.AsQueryable().Where(it=>it.Id==targetId)
            .Includes(it=>it.Service).FirstAsync();
        var status = await _deployTargetService.GetServiceStatus(deployTarget);
        return Ok(status);
    }
    /// <summary>
    /// 查询发布目标
    /// </summary>
    /// <returns></returns>
    [HttpGet("query")]
    public Task<List<DeployTarget>> Query([FromQuery]int? serviceId)
    {
        return _deployTargetService.Repository.AsQueryable()
            .WhereIF(serviceId is > 0,it => it.ServiceId==serviceId)
            .ToListAsync();
    }
    
    /// <summary>
    /// 插入发布目标
    /// </summary>
    /// <returns></returns>
    [HttpPost("insert")]
    public Task<bool> Insert([FromBody]DeployTargetDto deployTargetDto)
    {
        var service = deployTargetDto.Adapt<DeployTarget>();
        return _deployTargetService.Repository.InsertAsync(service);
    }
    
    /// <summary>
    /// 更新
    /// </summary>
    /// <returns></returns>
    [HttpPost("update")]
    public async Task<bool> Update([FromBody]DeployTargetDto deployTargetDto)
    {
        var item=await _deployTargetService.Repository.GetByIdAsync(deployTargetDto.Id);
        deployTargetDto.Adapt(item);
        return await _deployTargetService.Repository.UpdateAsync(item);
    }
    /// <summary>
    /// 删除
    /// </summary>
    /// <returns></returns>
    [HttpPost("delete")]
    public Task<bool> Delete([FromQuery]int id)
    {
        return  _deployTargetService.Repository.DeleteByIdAsync(id);
    }

}