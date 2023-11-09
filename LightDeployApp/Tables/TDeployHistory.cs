using System;
using SqlSugar;

namespace LightDeployApp.Tables;

/// <summary>
/// 部署历史
/// </summary>
public class TDeployHistory:ViewModelBase
{
    [ColumnName("部署时间")]
    public DateTime CreateTime { get; set; }

    [ColumnName("部署文件夹")]
    [SugarColumn(IsNullable = true)]
    public string? DeployFilesDir { get; set; }

    [ColumnName("备注说明")]
    public string Remark { get; set; }
    
    [ColumnName("服务名称")]
    public string ServiceName { get; set; }
}