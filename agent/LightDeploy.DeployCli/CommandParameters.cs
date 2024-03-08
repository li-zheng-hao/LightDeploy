using CommandLine;

namespace LightDeploy.DeployCli;

public class CommandParameters
{
    [Value(0,Required = false,Default = "publish",HelpText = "操作名称,支持：publish")]
    public string Operation { get; set; } = null!;
    
    [Option('s',"service",Required = true,HelpText = "服务名")]
    public string Service { get; set; }
    
    [Option('e',"environment",Required = true,HelpText = "环境名")]
    public string Environment { get; set; }
    
    [Option('d',"dir",Required = true,HelpText = "发布目录")]
    public string TargetProjectPath { get; set; }
}