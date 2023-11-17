using System.Management;
using System.ServiceProcess;
using System.Text.Json;
using System.Text.Json.Serialization;
using LightDeploy.ClientAgent.Dto;
using LightDeploy.ClientAgent.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;

namespace LightDeploy.ClientAgent.Controllers;

[ApiController]
[Authorize]
[Route("/api/[controller]/[action]")]
public class DeployController : ControllerBase
{
    private readonly ILogger<DeployController> _logger;
    private readonly DeployService _deployService;

    public DeployController(ILogger<DeployController> logger,DeployService deployService)
    {
        _logger = logger;
        _deployService = deployService;
    }

    [HttpPost]
    public async Task<IActionResult> Deploy([FromForm]DeployDto deployDto)
    {
        try
        {
            _logger.LogInformation($"请求更新服务:{deployDto.ServiceName}");
            await _deployService.Deploy(deployDto);
        }
        catch (BusinessException e)
        {
            _logger.LogError(e.Message);
            return BadRequest(e.Message);
        }
        return Ok();
    }

    [HttpPost()]
    public IActionResult Compare([FromQuery] string serviceName,[FromBody] List<FileInfoDto> fileInfoDtos)
    {
        try
        {
            List<FileInfoDto> newFileInfos = _deployService.CompareFileInfos(serviceName,fileInfoDtos);
            return Ok(newFileInfos);
        }
        catch (BusinessException e)
        {
            return BadRequest(e.Message);
        }
    }
   

    [HttpGet,AllowAnonymous]
    public IActionResult Ping()
    {
        return Ok();
    }

    [HttpPost()]
    public IActionResult UpdateSelf()
    {
        var file=HttpContext.Request.Form.Files["file"];
        var parent = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory);
        var targetPath=Path.Combine(parent!.Parent.FullName, "LightDeployUpdateService","UpdatePackages");
        if (!Directory.Exists(targetPath))
        {
            Directory.CreateDirectory(targetPath);
        }
        var targetFile = Path.Combine(targetPath, file!.FileName);
        if (System.IO.File.Exists(targetFile))
        {
            return BadRequest("更新文件已存在,请稍后再试");
        }
        using var stream = System.IO.File.Create(targetFile);
        file.CopyTo(stream);
        return Ok();
    }

    /// <summary>
    /// 检查Windows服务是否存在
    /// </summary>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    [HttpPost]
    public IActionResult CheckServiceExist([FromQuery]string serviceName)
    {
        var exist=WindowsServiceHelper.ServiceIsExisted(serviceName);
        return Ok(exist);
    }

    /// <summary>
    /// 安装windows服务
    /// </summary>
    /// <param name="installWindowsServiceDto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> InstallWindowsService([FromForm] InstallWindowsServiceDto installWindowsServiceDto)
    {
        var exist=WindowsServiceHelper.ServiceIsExisted(installWindowsServiceDto.ServiceName);
        if (exist)
            return BadRequest("服务已存在");
        var result=await _deployService.InstallWindowsService(installWindowsServiceDto);
        if(result)
            return Ok();
        return BadRequest("安装失败"); 
    }

    [HttpPost]
    public IActionResult StartService([FromQuery] string serviceName)
    {
        var exist=WindowsServiceHelper.ServiceIsExisted(serviceName);
        if (!exist)
            return BadRequest("服务不存在");
        WindowsServiceHelper.StartService(serviceName);
        return Ok();
    }
    [HttpPost]
    public IActionResult StopService([FromQuery] string serviceName)
    {
        var exist=WindowsServiceHelper.ServiceIsExisted(serviceName);
        if (!exist)
            return BadRequest("服务不存在");
        WindowsServiceHelper.StopService(serviceName);
        return Ok();

    }

    /// <summary>
    /// 获取服务状态
    /// </summary>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    [HttpGet()]
    public IActionResult GetStatus([FromQuery] string serviceName)
    {
        var exist=WindowsServiceHelper.ServiceIsExisted(serviceName);
        if (!exist)
            return BadRequest("服务不存在");
        return Ok(WindowsServiceHelper.GetStatus(serviceName));

    }
}