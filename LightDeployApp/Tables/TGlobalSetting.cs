using System;
using SqlSugar;

namespace LightDeployApp.Tables;

/// <summary>
/// 全局配置
/// </summary>
public class TGlobalSetting:ViewModelBase
{
    [ColumnName("编号")]
    [SugarColumn(IsPrimaryKey = true,IsIdentity=true)]
    public int Id { get; set; }

    [ColumnName("企业微信Key")]
    [SugarColumn(IsNullable = true)]
    public string? QiyeWeChatKey { get; set; }
}