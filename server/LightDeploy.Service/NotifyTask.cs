using LightApi.Infra;
using Serilog;

namespace LightDeploy.Service;

public static class NotifyTask
{
    private static readonly INotificationsServerSentEventsService? _serverSentEventsService;

    public static List<string> _messages = new();
    
    public static object locker = new object();

    static NotifyTask()
    {
        _serverSentEventsService = App.GetService<INotificationsServerSentEventsService>();
        Task.Run(Notify);
    }

    private static void Notify()
    {
        while (true)
        {
            Thread.Sleep(1000);
            
            if (_messages.Count == 0)
            {
#if DEBUG
                Log.Information("未扫描到需要发送的消息");   
#endif
                continue;
            }
        
            string msg;
        
            lock (locker)
            {
                msg = string.Join( "|",_messages);
                _messages.Clear();
            }

            try
            {
                _serverSentEventsService?.SendEventAsync(msg).GetAwaiter().GetResult();

            }
            catch (Exception e)
            {
                Log.Error(e,"发送消息失败");
            }
        }
       
    }

    public static void AddNotify(string msg)
    {
        lock (locker)
        {
            _messages.Add(msg);
        }
    }
}