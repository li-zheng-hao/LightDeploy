using System.Management;
using System.Text.Json;
using System.Text.Json.Serialization;
using LightDeploy.ClientAgent.Dto;
using LightDeploy.ClientAgent.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;

namespace LightDeploy.ClientAgent.Controllers;

[ApiController]
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

    [HttpPost]
    public IActionResult TestDeploy()
    {
        var location = WindowServiceHelper.GetWindowsServiceLocation("TestService");
        return Ok(location);
    }
 
    [HttpGet]
    public IActionResult ListFileInfo([FromQuery] string serviceName, [FromQuery] string ignoreFileExtensions="")
    {
        try
        {
            List<FileInfoDto> fileInfoDtos = _deployService.GetFileInfos(serviceName,ignoreFileExtensions);
            
            return Ok(fileInfoDtos);
        }
        catch (BusinessException e)
        {
            return BadRequest(e.Message);
        }
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
   

    [HttpGet]
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
}