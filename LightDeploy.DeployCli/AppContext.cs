using LightDeploy.DeployCli;
using LightDeploy.DeployCli.Tables;

namespace LightDeployApp;

public static class AppContext
{
    private static AppDataContext DataContext { get; set; }
    public static AppDataContext GetAppDataContext()
    {
        if (DataContext == null)
        {
            DataContext ??= new AppDataContext();
            var _services = DBHelper.GetClient()
                .Queryable<TService>()
                .ToList();
            
            var _environments= DBHelper.GetClient()
                .Queryable<TEnvironment>()
                .ToList();
            DataContext.Services = _services;
            DataContext.Environments= _environments;
        }
        return DataContext;
    }
   
}