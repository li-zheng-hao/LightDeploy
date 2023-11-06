using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using LightDeployApp;
using Microsoft.VisualBasic.Logging;
using AppContext = LightDeployApp.AppContext;

namespace SW.Core.Helper;

public static class ProcessorHelper
{
    /// <summary>
    /// 原生调用外部进程 路径可以有空格
    /// </summary>
    /// <param name="exe"></param>
    /// <param name="param"></param>
    /// <param name="ignoreException"></param>
    /// <param name="waitSeconds"></param>
    public static async Task InvokeAsync(string exe, string param, bool ignoreException = false,int waitSeconds = 600)
    {
        try
        {
            Process process = new Process();
            process.StartInfo.FileName = exe;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardInput = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.ErrorDialog = false;
            process.StartInfo.Arguments = param;
            process.EnableRaisingEvents = false;
            process.StartInfo.StandardOutputEncoding =
                Encoding.GetEncoding(65001 );
            process.StartInfo.StandardErrorEncoding=         Encoding.GetEncoding(65001 );
            process.Start();

            

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(waitSeconds * 1000);
            var task1= Task.Run(() =>
            {
                try
                {
                    while (!process.StandardOutput.EndOfStream)
                    {
                        string line = process.StandardOutput.ReadLine();
                        // do something with line
                        AppContext.GetAppDataContext().Log(line);
                    }
                }
                catch (Exception)
                {
                }
            });

            var task2=Task.Run(() =>
            {
                try
                {
                    while (!process.StandardError.EndOfStream)
                    {
                        string line = process.StandardError.ReadLine();
                        // do something with line
                        // await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
                        // {
                        AppContext.GetAppDataContext().Log(line);
                        // });
                    }
                }
                catch (Exception)
                {
                }
            });
            Task task3= process.WaitForExitAsync(cancellationTokenSource.Token);
            await Task.WhenAll(task1, task2, task3);
        }
        catch (Exception ex)
        {
            //https://stackoverflow.com/questions/61935980/process-finished-with-exit-code-1073740940-0xc0000374
            if (ignoreException)
            {    Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    AppContext.GetAppDataContext().Log($"此异常已被忽略 {ex.Message}");
                });

                return;
            }
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                AppContext.GetAppDataContext().Log($"进程调用异常 {ex.Message}");
            });

            throw;
        }
    }
   
}