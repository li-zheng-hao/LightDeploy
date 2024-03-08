using Serilog;
using Serilog.Configuration;

namespace LightDeploy.ClientAgent.Serilog;


public static class AgentSinkExtension
{
    public static LoggerConfiguration AgentSink(
        this LoggerSinkConfiguration loggerConfiguration,
        IFormatProvider formatProvider = null)
    {
        return loggerConfiguration.Sink(new AgentSink(formatProvider));
    }
}