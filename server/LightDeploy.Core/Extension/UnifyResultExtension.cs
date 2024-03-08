using LightApi.Infra.Unify;
using Newtonsoft.Json;

namespace LightDeploy.Core.Extension;

public static class UnifyResultExtension
{
    public static T? GetData<T>(this UnifyResult unifyResult)
    {
        var str=JsonConvert.SerializeObject(unifyResult.data);
        return (T?)JsonConvert.DeserializeObject(str,typeof(T));
    }
}