using LightApi.SqlSugar;
using SqlSugar;

namespace LightDeploy.Domain;

public class GlobalSetting:ISugarTable
{
    
    [SugarColumn(IsPrimaryKey = true,IsIdentity=true)]
    public int Id { get; set; }

    [SugarColumn(IsNullable = true)]
    public string? QiyeWeChatKey { get; set; }
}