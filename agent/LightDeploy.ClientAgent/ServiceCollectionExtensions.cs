using System.Net;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using LightApi.Infra.DependencyInjections;
using LightApi.SqlSugar;
using LightDeploy.ClientAgent.Serilog;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using SqlSugar;

namespace LightDeploy.ClientAgent;

public static class ServiceCollectionExtensions
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
                    ConnectionString = "DataSource=lightdeploy_agent.db",
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
        string logTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}]  {Message:lj} {NewLine} ";
        var config = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .WriteTo.Console(outputTemplate: logTemplate)
            .WriteTo.Conditional(it=>it.Properties.ContainsKey("WsConnectionId"),
                it=>it.AgentSink())
            .WriteTo.File("logs/lightdeploy_.log"
                , rollingInterval: RollingInterval.Day, outputTemplate: logTemplate);
        var logger = config.CreateLogger();
        Log.Logger = logger;
        builder.Host.UseSerilog(logger, dispose: true);
        return builder;
    }
    public static IServiceCollection AddInfraSetup(this IServiceCollection serviceCollection,IConfiguration configuration)
    {
        serviceCollection.AddInfrastructure(configuration, configure =>
        {
            configure.ConfigInfrastructureOptions(it =>
            {
                it.EnableGlobalModelValidator = true;
                it.EnableGlobalUnifyResult = true;
                it.EnableGlobalExceptionFilter = true;
                it.DefaultModelValidateErrorHttpStatusCode = HttpStatusCode.OK;
                it.UseFirstModelValidateErrorMessage = true;
            });
        });

        return serviceCollection;
    }
    /// <summary>
    /// 使用Autofac注入服务
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IHostBuilder AddAutofacSetup(this IHostBuilder builder,IConfiguration configuration)
    {
        builder
            .UseServiceProviderFactory<
                ContainerBuilder>(new AutofacServiceProviderFactory());
        return builder;
    }
    public static WebApplication AddConnectionIdMiddleware(this WebApplication app)
    {
        app.Use(async (httpContext, next) =>
        {
            
            if (httpContext.Request.Headers.TryGetValue("WsConnectionId", out var connectionId))
            {
                using (LogContext.PushProperty("WsConnectionId", connectionId.ToString()))
                {
                    await next();
                }
              
            }
            else
            {
                await next();
            }
        });
        return app;
    }
}