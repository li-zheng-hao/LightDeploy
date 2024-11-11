using System.Reflection;
using LightApi.EFCore.Config;

namespace LightDeploy.ClientAgent.Domain;

public class LdAgentAppDbEntityInfo:AbstractSharedEntityInfo
{
    protected override Assembly GetCurrentAssembly()
    {
        return typeof(LdAgentAppDbEntityInfo).Assembly;
    }
}