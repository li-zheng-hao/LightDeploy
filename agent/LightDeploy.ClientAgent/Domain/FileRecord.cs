
using LightApi.EFCore.Entities;

namespace LightDeploy.ClientAgent.Domain;

/// <summary>
/// 文件md5记录
/// </summary>
public class FileRecord:IEfEntity
{
    public int Id { get; set; }
    
    /// <summary>
    /// 服务名
    /// </summary>
    public string ServiceName { get; set; }
    
    /// <summary>
    /// 发布时间戳
    /// </summary>
    public long PublishTimestamp { get; set; }
    
    /// <summary>
    /// 相对路径文件夹(服务所在文件夹或发布目录)
    /// </summary>
    public string RelativeDirectory { get; set; }
    
    /// <summary>
    /// 绝对路径文件夹
    /// </summary>
    public string AbsoluteDirectory { get; set; }

    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; set; }
    
    /// <summary>
    /// 文件MD5
    /// </summary>
    public string MD5 { get; set; }
}