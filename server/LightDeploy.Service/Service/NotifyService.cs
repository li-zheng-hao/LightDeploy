using Lib.AspNetCore.ServerSentEvents;
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
    /// 发送消息给当前用户
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public Task NotifyMessageToUser(string message)
    {
        var num=_serverSentEventsService.GetClients().Count();
        if(num<=0) Log.Warning("当前无用户在线");
        message = $"{DateTime.Now} {message}";
        return _serverSentEventsService.SendEventAsync(message);
    }

  
}

