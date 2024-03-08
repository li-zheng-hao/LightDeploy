namespace LightDeploy.Service.Dto;

public class InstallServiceRequest
{
    /// <summary>
    /// 服务所在目录（目标机器）
    /// </summary>
    public string TargetDir { get; set; }
    
    /// <summary>
    /// 执行参数
    /// </summary>
    public string ExeParams { get; set; }
    
    /// <summary>
    /// 目标路径(相对服务所在目录)
    /// </summary>
    public string ExePath { get; set; }

    /// <summary>
    /// 服务描述
    /// </summary>
    public string Description { get; set; }
    
}