using LightApi.EFCore.Entities;

namespace LightDeploy.Server.Domain;

public class GlobalSetting:IEfEntity
{
    
    public int Id { get; set; }

    public string? QiyeWeChatKey { get; set; }
}