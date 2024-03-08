using Asp.Versioning;
using LightDeploy.Core.FileProvider;
using LightApi.Infra;
using Microsoft.AspNetCore.Mvc;

namespace LightDeploy.Api.Controllers.v2;

/// <summary>
/// 文件
/// </summary>
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class FileController : ControllerBase
{
    [HttpGet("test")]
    public string test()
    {
        return "from v2";
    }
 
    
}

