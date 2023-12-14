namespace LightDeployApp.Dtos;

public class UpdateDeployDto:ViewModelBase
{
    [ColumnName("环境名称")]
    public string Name { get; set; }
    
    [ColumnName("地址")]
    public string Host { get; set; }

    [ColumnName("端口")]
    public string Port { get; set; }
    
    [ColumnName("是否需要更新")]
    public bool NeedDeploy { get; set; } = true;
    [ColumnName("AuthKey")]
    public string? AuthKey { get; set; }
}