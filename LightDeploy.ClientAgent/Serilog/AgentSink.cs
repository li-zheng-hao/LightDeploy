using LightApi.Infra;
using LightDeploy.ClientAgent.Hubs;
using Microsoft.AspNetCore.SignalR;
using Serilog.Core;
using Serilog.Events;

namespace LightDeploy.ClientAgent.Serilog;

public class AgentSink : ILogEventSink
{
    private readonly IFormatProvider _formatProvider;

    public AgentSink(IFormatProvider formatProvider)
    {
        _formatProvider = formatProvider;
    }

    public void Emit(LogEvent logEvent)
    {
        if (logEvent.Properties.TryGetValue("WsConnectionId", out var connectionId))
        {
            var con=connectionId as ScalarValue;
            var connectionIdStr=con?.Value as string;
            var hubContexts = App.GetService<IHubContext<DeployHub>>();
            var message = logEvent.RenderMessage(_formatProvider);
            hubContexts?.Clients.Client(connectionIdStr??string.Empty).SendAsync("Log", message).GetAwaiter().GetResult();
        }
        
      
    }
}