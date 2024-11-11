using System.Reflection;
using LightApi.EFCore.Config;

namespace LightDeploy.Server.Core;

public class AppEntityInfo:AbstractSharedEntityInfo
{
    protected override Assembly GetCurrentAssembly()
    {
        return Assembly.GetExecutingAssembly();
    }
}