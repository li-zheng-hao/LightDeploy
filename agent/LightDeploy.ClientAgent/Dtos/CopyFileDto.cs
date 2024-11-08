namespace LightDeploy.ClientAgent.Dto;

public class CopyFileDto
{
    /// <summary>
    /// Zip打包的文件
    /// </summary>
    public IFormFile File { get; set; }
    
    /// <summary>
    /// 解压的目标文件夹
    /// </summary>
    public string TargetDir { get; set; }
}