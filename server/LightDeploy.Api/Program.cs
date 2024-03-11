using System.Text;
using Asp.Versioning;
using Lib.AspNetCore.ServerSentEvents;
using LightDeploy.Core;
using LightDeploy.Core.Options;
using LightDeploy.Domain;
using LightApi.Infra;
using LightApi.Infra.DependencyInjections;
using LightApi.Infra.Helper;
using LightDeploy.Api;
using LightDeploy.Core.Middleware;
using LightDeploy.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.FileProviders;
using Prometheus;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerUI;
using ITimer = System.Threading.ITimer;

try
{
    var builder = WebApplication.CreateBuilder(args);

    #region 注入配置

    builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection(DatabaseOptions.SectionName));
    builder.Services.Configure<FileStorageOptions>(builder.Configuration.GetSection(FileStorageOptions.SectionName));
    builder.Services.Configure<SecretOptions>(builder.Configuration.GetSection(SecretOptions.SectionName));
    builder.Services.Configure<ThirdPartOptions>(builder.Configuration.GetSection(ThirdPartOptions.SectionName));

    #endregion
    
    #region 注册服务
    
    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);//注册Nuget包System.Text.Encoding.CodePages中的编码到.NET Core
    builder.Services.AddControllers();
    builder.Services.AddSwaggerGenSetup(new List<string>(){"LightDeploy.Api","LightDeploy.Service","LightDeploy.Domain"});
    
    builder.Services.AddEndpointsApiExplorer();

    // 支持接口Url版本控制
    builder.Services.AddApiVersionSetup();
    
    // Serilog日志
    builder.AddSerilogSetup(); 
    
    // 基础框架
    builder.Services.AddInfraSetup(builder); 

    // 跨域
    builder.Services.AddCorsSetup(); 
    
    // 权限认证
    builder.Services.AddAuthorizeSetup(); 
    
    // SqlSugar
    builder.Services.AddSqlSugarSetup(); 
    
    // 缓存
    builder.Services.AddEasyCachingSetup(); 
    
    // 分布式锁
    builder.Services.AddDistributedLockSetup(); 
    
    builder.Services.AddHttpContextAccessor();

    builder.Services.AddHostedService<DeployScheduler>();
    builder.Services.AddHostedService<BrowserStartTask>();
    
    // sse支持    
    builder.Services.AddServerSentEvents<INotificationsServerSentEventsService, NotificationsServerSentEventsService>(
        opt =>
        {
            opt.KeepaliveContent = "ping";
            opt.KeepaliveInterval=15;
            opt.KeepaliveMode = ServerSentEventsKeepaliveMode.Always;

        });


    // MVC配置
    builder.Services.AddMvcSetup(); 

    // IOC容器 及批量注入
    builder.Host.AddAutofacSetup(builder.Configuration); 

    // Kestrel配置
    builder.WebHost.ConfigureKestrel(it =>
    {
        // 上传文件大小500MB
        it.Limits.MaxRequestBodySize = 524288000;
    }); 
    

    #endregion

    var app = builder.Build();

    #region 中间件

    // Swagger
    // if (!app.Environment.IsProduction())
    {
        app.UseSwagger();
        app.UseSwaggerUI(config =>
        {
            var descriptions = app.DescribeApiVersions();
            // Build a swagger endpoint for each discovered API version
            foreach (var description in descriptions)
            {
                var url = $"/swagger/{description.GroupName}/swagger.json";
                var name = description.GroupName.ToUpperInvariant();
                config.SwaggerEndpoint(url, name);
            }
            config.DocExpansion(DocExpansion.None);
            config.ConfigObject.AdditionalItems.Add("persistAuthorization", "true");
            config.IndexStream = () =>
                new FileStream(Path.Combine(AppContext.BaseDirectory, "wwwroot", "swagger-index.html"), FileMode.Open,
                    FileAccess.Read);
           
        });
    }
    // spa缓存
    app.UseSpaNoCacheMiddleware();

    app.UseFileServer();
   

    // 基础框架
    app.UseInfrastructure();

    app.UseRouting();

    app.UseAuthentication();

    app.UseCors();

    
    app.UseAuthorization();

    app.UseHttpMetrics();

    if (!app.Environment.IsProduction())
        app.UseMiniProfiler();

    app.MapMetrics();

    app.MapServerSentEvents<NotificationsServerSentEventsService>("/api/sse");

    app.MapControllers();

    #endregion

    app.MapFallbackToFile("/index.html");
    app.Run();
}
catch (HostAbortedException)
{
    // ignore
}
catch (Exception exception)
{
    Log.Logger.Fatal(exception, $"程序启动失败 {exception.Message}");
    Log.CloseAndFlush();
    Environment.Exit(1);
}