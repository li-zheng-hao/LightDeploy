using System.Reflection;
using LightApi.SqlSugar;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SqlSugar;

namespace LightDeploy.Common;

public class DatabaseMigrateService:IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public DatabaseMigrateService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var _client=scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();
        Type[] types=Assembly.GetEntryAssembly()!.GetTypes()
            .Where(it=>typeof(ISugarTable).IsAssignableFrom(it))//命名空间过滤，当然你也可以写其他条件过滤
            .ToArray();//断点调试一下是不是需要的Type，不是需要的在进行过滤
     
        var diffString= _client.CodeFirst.GetDifferenceTables(types).ToDiffString();
        Log.Information($"本次数据库迁移差距:{diffString}");
        _client.CodeFirst.SetStringDefaultLength(200).InitTables(types);//这样一个表就能成功创建了
        Log.Information("数据库迁移完成");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}