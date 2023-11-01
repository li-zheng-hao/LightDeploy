using System.Text.Json;
using System.Text.Json.Serialization;
using LightDeploy.ClientAgent.Dto;
using LightDeploy.ClientAgent.Services;
using Microsoft.AspNetCore.Mvc;

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
    public IActionResult TestCopy()
    {
        _deployService.CopyFilesRecursively("E:\\Test\\A","E:\\Test\\B");
        
        return Ok();
    }

    [HttpGet]
    public IActionResult ListFileInfo([FromQuery] string serviceName)
    {
        try
        {
            List<FileInfoDto> fileInfoDtos = _deployService.GetFileInfos(serviceName);
            
            return Ok(fileInfoDtos);
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
}