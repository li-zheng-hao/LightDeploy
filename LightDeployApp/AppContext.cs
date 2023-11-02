using System;
using System.Linq;
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
        GetAppDataContext().Services = _services;
        GetAppDataContext().Environments= _environments;
        GetAppDataContext().EnvironmentNames = _environments.Select(it => it.Name).Distinct().ToList();
    }
}