using LightApi.SqlSugar;
using LightDeploy.Domain;
using Microsoft.AspNetCore.Mvc;

namespace LightDeploy.Api.Controllers;

/// <summary>
/// 设置
/// </summary>
[ApiController]
[Route("api/setting")]
public class GlobalSettingController : ControllerBase
{
    private readonly IBaseRepository<GlobalSetting> _repository;

    public GlobalSettingController(IBaseRepository<GlobalSetting> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 查询
    /// </summary>
    /// <returns></returns>
    [HttpGet("query")]
    public GlobalSetting Query()
    {
        return _repository.GetFirst(it=>true);
    }
    /// <summary>
    /// 更新 不存在则插入
    /// </summary>
    /// <param name="setting"></param>
    /// <returns></returns>
    [HttpPost("update")]
    public IActionResult Update([FromBody]GlobalSetting setting)
    {
        _repository.InsertOrUpdate(setting);
        return Ok();
    }
}