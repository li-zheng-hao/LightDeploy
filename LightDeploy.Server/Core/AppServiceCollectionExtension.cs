using System.Reflection;
using LightApi.SqlSugar;
using Serilog;
using Serilog.Events;
using SqlSugar;

namespace LightDeploy.Server.Core;

public static class AppServiceCollectionExtension
{

    /// <summary>
    /// SqlSugar配置
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddSqlSugarSetup(this IServiceCollection services)
    {
        services.AddScoped<ISqlSugarClient>(s =>
        {
            SqlSugarClient sqlSugar = new SqlSugarClient(
                new ConnectionConfig()
                {
                    DbType = DbType.Sqlite,
                    ConnectionString = "DataSource=lightdeploy.db",
                    IsAutoCloseConnection = true,
                    ConfigureExternalServices = new ConfigureExternalServices()
                    {
                        //注意:  这儿AOP设置不能少
                        EntityService = (c, p) =>
                        {
                            /***高版C#写法***/
                            //支持string?和string  
                            if (p.IsPrimarykey == false && new NullabilityInfoContext()
                                    .Create(c).WriteState is NullabilityState.Nullable)
                            {
                                p.IsNullable = true;
                            }
                        }
                    },
                }
                ,
                db =>
                {
                    db.Aop.OnLogExecuted = (sql, pars) =>
                    {
                        //执行时间超过1秒
                        if (db.Ado.SqlExecutionTime.TotalSeconds > 1)
                        {
                            //代码CS文件名
                            var fileName = db.Ado.SqlStackTrace.FirstFileName;
                            //代码行数
                            var fileLine = db.Ado.SqlStackTrace.FirstLine;
                            //方法名
                            var firstMethodName = db.Ado.SqlStackTrace.FirstMethodName;

                            Log.Warning($"检测到慢查询Sql,耗时{db.Ado.SqlExecutionTime.TotalMilliseconds}毫秒:\r\n" +
                                        $"代码文件名:{fileName} 行数:{fileLine} 方法名{firstMethodName}\r\n" +
                                        UtilMethods.GetSqlString(DbType.SqlServer, sql, pars));
                        }
                    };
                    db.Aop.OnLogExecuting = (sql, pars) =>
                    {
                        
                            // //获取无参数化SQL 对性能有影响，特别大的SQL参数多的，调试使用
                            // Log.Information(UtilMethods.GetSqlString(DbType.SqlServer, sql, pars));
                    };
                });
            return sqlSugar;
        });
        services.AddSugarRepository();
        services.AddHostedService<DatabaseMigrateService>();
        return services;
    }

    public static WebApplicationBuilder AddSerilogSetup(this WebApplicationBuilder builder)
    {
        var config = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.WithProperty("ENV", builder.Environment.EnvironmentName)
            .Enrich.WithProperty("System", "LightDeploy")
            .Enrich.FromLogContext()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Transaction", LogEventLevel.Debug)
            .WriteTo.Console()
            .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs/lightdeploy_.log")
                , rollingInterval: RollingInterval.Day);

        var logger = config.CreateLogger();
        Log.Logger = logger;
        builder.Host.UseSerilog(logger, dispose: true);
        return builder;
    }
}