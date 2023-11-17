namespace LightDeployApp.Dtos;

/// <summary>
/// 通用返回包装类
/// </summary>
public class UnifyResult<T>
{
    public bool Success { get; set; } 
    
    public int Code { get; set; }

    public string? Msg { get; set; }

    public T? Data { get; set; }
}