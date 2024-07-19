namespace LightDeploy.Server.Core;

/// <summary>
/// 通用返回包装类
/// </summary>
public class UnifyResult<T>
{
    public bool success { get; set; } 
    
    public int code { get; set; }

    public string? msg { get; set; }

    public T? data { get; set; }
}
