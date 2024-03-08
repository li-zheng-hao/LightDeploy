using System.Diagnostics;
using System.Runtime.InteropServices;
using CliWrap;

namespace LightDeploy.Api;

public class BrowserStartTask:BackgroundService
{
    protected override  Task ExecuteAsync(CancellationToken stoppingToken)
    {
        OpenBrowser("http://localhost:31000");
        return Task.CompletedTask;
    }
    
    public static void OpenBrowser(string url)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); // Works ok on windows
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("xdg-open", url);  // Works ok on linux
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", url); // Not tested
        }
        else
        {
            throw new Exception("未知的操作系统");
        }
    }
}