namespace LightDeploy.ClientAgent.Dto;

public class FileInfoDto
{
    /// <summary>
    /// 相对指定路径的相对目录
    /// </summary>
    public string RelativeDirectory { get; set; }
    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; set; }
    
    /// <summary>
    /// 文件大小
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime LastWriteTime { get; set; }

    public string? AbsoluteDirectory { get; set; }
    public string MD5 { get; set; }
}