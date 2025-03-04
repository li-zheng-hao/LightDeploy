using System;
using System.Text;
using Serilog;

namespace LightDeploy.Server.Core;

public static class QiyeWeChatNotifyHelper
{
    private static readonly HttpClient client = new HttpClient();

    private static readonly string NOTIFY_TEMPLATE = """
        {
            "msgtype": "markdown",
            "markdown": {
                "content": "{message}"
            }
        }
        """;
    private static readonly string NOTIFY_MESSAGE_TEMPLATE = """
         ## 发布环境
        {{environment}}
        ## 系统
        {{title}}
        ## 发布说明
        {{message}}
        """;

    public static async Task Notify(string key, string environment, string title, string message)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }
        string url = $"https://qyapi.weixin.qq.com/cgi-bin/webhook/send?key={key}";
        StringBuilder notifyMessage = new StringBuilder();
        notifyMessage.AppendLine($"## 发布环境");
        notifyMessage.AppendLine(environment);
        notifyMessage.AppendLine($"## 系统");
        notifyMessage.AppendLine(title);
        notifyMessage.AppendLine($"## 发布说明");
        notifyMessage.AppendLine(message);
        var response = await client.PostAsync(
            url,
            new StringContent(NOTIFY_TEMPLATE.Replace("{message}", notifyMessage.ToString()))
        );
        if (response.IsSuccessStatusCode)
        {
            Log.Information("企业微信通知发送成功");
        }
    }
}
