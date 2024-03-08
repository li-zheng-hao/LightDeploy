using System.Collections.Generic;

namespace LightDeploy.ClientAgent.Dto;

public class DeployDto
{
    /// <summary>
    /// zip压缩包
    /// </summary>
    public byte[] File { get; set; }
    
    /// <summary>
    /// Windows服务名
    /// </summary>
    public string ServiceName { get; set; }
    
    /// <summary>
    /// 忽略文件的后缀名
    /// </summary>
    public List<string>? IgnoreFileExtensions { get; set; }
}