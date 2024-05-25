using Lib.AspNetCore.ServerSentEvents;
using LightApi.Infra.Extension;
using LightDeploy.Core.Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Serilog;

namespace LightDeploy.Service;


public interface INotificationsServerSentEventsService : IServerSentEventsService
{ }

public class NotificationsServerSentEventsService : ServerSentEventsService, INotificationsServerSentEventsService
{
    public NotificationsServerSentEventsService(IOptions<ServerSentEventsServiceOptions<NotificationsServerSentEventsService>> options)
        : base(options.ToBaseServerSentEventsServiceOptions())
    { }
}


public class NotifyService :IScopedDependency
{
    private readonly INotificationsServerSentEventsService _serverSentEventsService;

    public NotifyService(INotificationsServerSentEventsService  serverSentEventsService)
    {
        _serverSentEventsService = serverSentEventsService;
    }

    /// <summary>
    /// 发布服务器
    /// </summary>
    private string Host { get; set; } = string.Empty;

    /// <summary>
    /// 设置发布主机
    /// </summary>
    /// <param name="host"></param>
    public void SetHost(string host)
    {
        Host = $"【{host}】";
    }

    public void ClearHost()
    {
        Host= string.Empty;
    }
    /// <summary>
    /// 发送消息给当前用户
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public Task NotifyMessageToUser(string message)
    {
        var num=_serverSentEventsService.GetClients().Count();
        if(num<=0) Log.Warning("当前无用户在线");
        message = $"{Host}{DateTime.Now} {message}";
        NotifyTask.AddNotify(message);
        return Task.CompletedTask;
    }

  
}

