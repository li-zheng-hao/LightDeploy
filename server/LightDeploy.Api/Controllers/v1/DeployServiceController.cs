using LightApi.Infra.Extension;
using LightDeploy.Domain;
using LightDeploy.Service;
using LightDeploy.Service.Dto;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using DeployService = LightDeploy.Domain.DeployService;

namespace LightDeploy.Api.Controllers;

/// <summary>
/// 服务
/// </summary>
[ApiController]
[Route("api/service")]
public class DeployServiceController : ControllerBase
{

    private readonly ILogger<DeployServiceController> _logger;
    private readonly DeployServiceService _deployServiceService;

    public DeployServiceController(ILogger<DeployServiceController> logger,DeployServiceService deployServiceService)
    {
        _logger = logger;
        _deployServiceService = deployServiceService;
    }
    
    /// <summary>
    /// 查询服务
    /// </summary>
    /// <returns></returns>
    [HttpGet("query")]
    public Task<List<DeployService>> Query([FromQuery]QueryServiceRequest request)
    {
        return _deployServiceService.Repository.AsQueryable()
            .WhereIF(request.ServiceName.IsNotNullOrWhiteSpace(), it => it.Name.Contains(request.ServiceName))
            .WhereIF(request.ServiceGroup.IsNotNullOrWhiteSpace(), it => it.GroupName.Contains(request.ServiceGroup))
            .WhereIF(request.Environment.IsNotNullOrWhiteSpace(),
                it => it.EnvironmentName==request.Environment)
            .ToListAsync();
    }
    
    /// <summary>
    /// 插入
    /// </summary>
    /// <returns></returns>
    [HttpPost("insert")]
    public Task<bool> Insert([FromBody]DeployServiceDto deployServiceDto)
    {
        var service = deployServiceDto.Adapt<DeployService>();
        FileAttributes attr = System.IO.File.GetAttributes(deployServiceDto.ProjectPath);

        if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
        {
            Check.ThrowIf(!Directory.Exists(deployServiceDto.ProjectPath),"目录不存在");
        }
        else
            Check.ThrowIf(!System.IO.File.Exists(deployServiceDto.ProjectPath),"项目文件不存在");

        return _deployServiceService.Repository.InsertAsync(service);
    }
    
    /// <summary>
    /// 更新
    /// </summary>
    /// <returns></returns>
    [HttpPost("update")]
    public async Task<bool> Update([FromBody]DeployServiceDto deployServiceDto)
    {
        var item=await _deployServiceService.Repository.GetByIdAsync(deployServiceDto.Id);
        deployServiceDto.Adapt(item);
        return await _deployServiceService.Repository.UpdateAsync(item);
    }
    /// <summary>
    /// 删除
    /// </summary>
    /// <returns></returns>
    [HttpPost("delete")]
    public Task<bool> Delete([FromQuery]int id)
    {
        return  _deployServiceService.Repository.DeleteByIdAsync(id);
    }

}