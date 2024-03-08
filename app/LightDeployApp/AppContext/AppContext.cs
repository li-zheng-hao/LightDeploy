using System;
using System.Collections.ObjectModel;
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
            .OrderBy(it=>it.GroupName)
            .OrderBy(it=>it.Name)
            .ToList();
            
        var _environments= DBHelper.GetClient()
            .Queryable<TEnvironment>()
            .ToList();
        var globalSetting = DBHelper.GetClient().Queryable<TGlobalSetting>().First() ?? DBHelper.GetClient().Insertable(new TGlobalSetting()).ExecuteReturnEntity();
        GetAppDataContext().GlobalSetting = globalSetting;
        
        GetAppDataContext().Services = _services;
        GetAppDataContext().ServiceGroupNames = _services.Select(it => it.GroupName).Where(it=>!string.IsNullOrWhiteSpace(it)).Distinct().ToList()!;
        GetAppDataContext().Targets= _environments;
        GetAppDataContext().EnvironmentNames= _environments.Select(it=>it.Name).Distinct().ToList();
        GetAppDataContext().ServicesView = CollectionViewSource.GetDefaultView(_services);
        GetAppDataContext().ServicesView.GroupDescriptions!.Add(new PropertyGroupDescription("GroupName"));
        GetAppDataContext().Environments = _services.Where(it=>!string.IsNullOrWhiteSpace(it.EnvironmentName)).Select(it => it.EnvironmentName!).Distinct().ToList() ;
    }

    public static async Task RefreshHistory(int id)
    {
        var data=await DBHelper.GetClient().Queryable<TDeployHistory>()
            .Where(it => it.ServiceId == id)
            .OrderByDescending(it => it.CreateTime)
            .ToListAsync();
        GetAppDataContext().DeployHistories = data;
    }

    public static void Log(string? msg)
    {
        GetAppDataContext().Log(msg);
    }
}