using Serilog;

namespace LightDeploy.Server.Services;

public class NotifyService
{
    public List<string> Logs { get; private set; } = new();

    public static object locker = new();
    public void ClearLogs()
    {
        Logs.Clear();
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
    public void NotifyMessageToUser(string message)
    {
        lock (locker)
        {
            message = $"{Host}{DateTime.Now} {message}";
            Logs.Insert(0,message);
            Log.Information(message);
        }
    }
}