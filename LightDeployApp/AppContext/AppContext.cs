using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using LightDeployApp.Tables;

namespace LightDeployApp;

public static class AppContext
{
    public static IServiceProvider ServiceProvider { get; set; }

    public static AppDataContext GetAppDataContext()
    {
        return (ServiceProvider.GetService(typeof(AppDataContext)) as AppDataContext)!;
    }

    public static void RefreshData()
    {
        var _services = DBHelper.GetClient()
            .Queryable<TService>()
            .ToList();
            
        var _environments= DBHelper.GetClient()
            .Queryable<TEnvironment>()
            .ToList();
        var globalSetting = DBHelper.GetClient().Queryable<TGlobalSetting>().First() ?? DBHelper.GetClient().Insertable(new TGlobalSetting()).ExecuteReturnEntity();
        GetAppDataContext().GlobalSetting = globalSetting;
        
        GetAppDataContext().Services = _services;
        GetAppDataContext().Environments= _environments;
        GetAppDataContext().EnvironmentNames= _environments.Select(it=>it.Name).Distinct().ToList();
        GetAppDataContext().ServicesView = new ListCollectionView(_services);
        GetAppDataContext().ServicesView.GroupDescriptions!.Add(new PropertyGroupDescription("GroupName"));
    }

    public static async Task RefreshHistory(string deployParamsServiceName)
    {
        var data=await DBHelper.GetClient().Queryable<TDeployHistory>()
            .Where(it => it.ServiceName == deployParamsServiceName)
            .OrderByDescending(it => it.CreateTime)
            .ToListAsync();
        GetAppDataContext().DeployHistories = data;
    }

    public static void Log(string? msg)
    {
        GetAppDataContext().Log(msg);
    }
}