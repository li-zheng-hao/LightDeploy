namespace LightDeployApp.Dtos;

public class QiyeWeChatNotifyDto
{
    public string msgtype { get; set; } = "markdown";

    public MarkdownBody markdown { get; set; }
}
public class MarkdownBody
{
    public string content { get; set; }
}