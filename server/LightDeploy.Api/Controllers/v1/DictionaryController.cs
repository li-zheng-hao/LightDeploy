using Microsoft.AspNetCore.Mvc;

namespace LightDeploy.Api.Controllers;

[ApiController]
[Route("api/dic")]
public class DictionaryController : ControllerBase
{
    [HttpGet("environments"),ResponseCache(Duration = 60)]
    public string[] Envrionments()
    {
        return new[] {"开发", "开发测试", "测试", "预览", "生产"};
    }
    
    
}