using LightApi.Infra.Extension;
using LightApi.SqlSugar;
using LightDeploy.Domain;
using LightDeploy.Service;
using LightDeploy.Service.Dto;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace LightDeploy.Api.Controllers;

/// <summary>
/// 发布历史
/// </summary>
[ApiController]
[Route("api/history")]
public class DeployHistoryController : ControllerBase
{

    private readonly ILogger<DeployServiceController> _logger;
    private readonly IBaseRepository<DeployHistory> _repository;

    public DeployHistoryController(ILogger<DeployServiceController> logger,IBaseRepository<DeployHistory> repository)
    {
        _logger = logger;
        _repository = repository;
    }
    
    /// <summary>
    /// 查询
    /// </summary>
    /// <returns></returns>
    [HttpGet("query")]
    public Task<List<DeployHistory>> Query([FromQuery]int? serviceId)
    {
        if (serviceId != null)
        {
            return _repository.AsQueryable()
                .WhereIF(serviceId is > 0,it => it.ServiceId==serviceId)
                .OrderByDescending(it=>it.PublishTime)
                .Take(10)
                .ToListAsync();
        }
        return _repository.AsQueryable()
            .ToListAsync();
       
    }
    
    /// <summary>
    /// 插入
    /// </summary>
    /// <returns></returns>
    [HttpPost("insert")]
    public Task<bool> Insert([FromBody]DeployHistory history)
    {
        return _repository.InsertAsync(history);
    }
    
    /// <summary>
    /// 更新
    /// </summary>
    /// <returns></returns>
    [HttpPost("update")]
    public  Task<bool> Update([FromBody]DeployHistory deployHistory)
    {
        return _repository.UpdateAsync(deployHistory);
    }
    /// <summary>
    /// 删除
    /// </summary>
    /// <returns></returns>
    [HttpPost("delete")]
    public Task<bool> Delete([FromQuery]int id)
    {
        return  _repository.DeleteByIdAsync(id);
    }

}