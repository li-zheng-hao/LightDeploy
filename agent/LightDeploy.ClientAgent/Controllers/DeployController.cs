using LightApi.Infra;
using LightApi.Infra.Extension;
using LightApi.Infra.InfraException;
using LightDeploy.ClientAgent.Dto;
using LightDeploy.ClientAgent.Services;
using LightDeploy.Common.Helper;
using Masuit.Tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

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
        _logger.LogInformation($"请求更新服务:{deployDto.ServiceName}");
        await _deployService.Deploy(deployDto);
       
        return Ok();
    }
    
    /// <summary>
    /// 和目录比较文件
    /// </summary>
    /// <param name="targetDir"></param>
    /// <param name="fileInfoDtos"></param>
    /// <returns></returns>
    [HttpPost()]
    public IActionResult CompareDir([FromQuery] string targetDir,[FromBody] List<FileHelper.FileInfoDto> fileInfoDtos)
    {
        List<FileHelper.FileInfoDto> newFileInfos = _deployService.CompareFileInfosInDir(targetDir,fileInfoDtos);
        return Ok(newFileInfos);
    }
    
    [HttpPost()]
    public IActionResult Compare([FromQuery] string serviceName,[FromBody] List<FileHelper.FileInfoDto> fileInfoDtos)
    {
        List<FileHelper.FileInfoDto> newFileInfos = _deployService.CompareFileInfos(serviceName,fileInfoDtos);
        return Ok(newFileInfos);
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
            throw new BusinessException("更新文件已存在,请稍后再试");
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
        Log.Information(installWindowsServiceDto.ToJsonString());
        var exist=WindowsServiceHelper.ServiceIsExisted(installWindowsServiceDto.ServiceName);
        Check.ThrowIf(exist,"服务已存在");
        var result=await _deployService.InstallWindowsService(installWindowsServiceDto);
        Check.ThrowIf(result,"安装失败");
        return Ok();
    }

    [HttpPost]
    public IActionResult StartService([FromQuery] string serviceName)
    {
        var exist=WindowsServiceHelper.ServiceIsExisted(serviceName);
        Check.ThrowIf(!exist,"服务不存在");
        WindowsServiceHelper.StartService(serviceName);
        return Ok();
    }
    [HttpPost]
    public IActionResult StopService([FromQuery] string serviceName)
    {
        var exist=WindowsServiceHelper.ServiceIsExisted(serviceName);
        Check.ThrowIf(!exist,"服务不存在");
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
        Check.ThrowIf(!exist,"服务不存在");
        return Ok(WindowsServiceHelper.GetStatus(serviceName));

    }

    /// <summary>
    /// 获取代理版本
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult GetAgentVersion()
    {
        return Ok(App.GetConfig<string>("Version")??"未知");
    }

    /// <summary>
    /// 复制文件
    /// </summary>
    /// <param name="copyDto"></param>
    /// <returns></returns>
    [HttpPost]
    public IActionResult Copy([FromForm] CopyFileDto copyDto)
    {
        _deployService.CopyFile(copyDto);
        return Ok();
    }
}