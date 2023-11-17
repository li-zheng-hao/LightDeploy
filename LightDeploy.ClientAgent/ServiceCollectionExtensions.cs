using System.Net;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using LightApi.Infra.DependencyInjections;
using LightDeploy.ClientAgent.Serilog;
using Serilog;
using Serilog.Context;
using Serilog.Events;

namespace LightDeploy.ClientAgent;

public static class ServiceCollectionExtensions
{
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