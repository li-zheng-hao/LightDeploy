using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Flurl;
using Flurl.Http;
using LightDeploy.ClientAgent.Dto;
using LightDeployApp.Dtos;
using LightDeployApp.Services;
using LightDeployApp.Tables;
using LightDeployApp.Windows;
using Masuit.Tools;
using Microsoft.AspNetCore.SignalR.Client;
using SevenZipExtractor;
using SqlSugar;
using SW.Core.Helper;

namespace LightDeployApp;

public class DeployService
{
    static HubConnection? connection;

    public static async Task<bool> Deploy(DeployParams deployParams)
    {
        try
        {
            if (deployParams.BuildMode == 0)
                return await DeployProject(deployParams);
            else
                return await DeployFolder(deployParams);
        }
        catch (Exception e)
        {
            AppContext.GetAppDataContext().Log(e.Message);
            return false;
        }
    }

    private static async Task<bool> DeployProject(DeployParams deployParams)
    {
        var tmpDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tmp", Guid.NewGuid().ToString("N"));
        var isSelfContained = deployParams.IsSelfContained ? "--self-contained" : string.Empty;
        await ProcessorHelper.InvokeAsync("dotnet",
            $" publish {deployParams.TargetPath} -c Release -o {tmpDir} {isSelfContained} ", true);
        await Task.Delay(2000);
        AppContext.GetAppDataContext().Log("编译完成");
        try
        {
            deployParams.TargetPath = tmpDir;
            return await DeployToServer(deployParams);
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }

    private static async Task<bool> DeployFolder(DeployParams deployParams)
    {
        return await DeployToServer(deployParams);
    }

    private static async Task<bool> DeployToServer(DeployParams deployParams)
    {
        var selectedEnvironments = AppContext.GetAppDataContext().SelectedEnvironments
            .Where(it => it.NeedDeploy).ToList();
        List<FileInfoDto>? calculateNeedDeployFiles = null;
        MemoryStream memoryStream = null;
        foreach (var environment in selectedEnvironments)
        {
            await using var agentHttpService = new AgentHttpService(environment);
            if (AppContext.GetAppDataContext().StopToken!.IsCancellationRequested) return false;
            AppContext.GetAppDataContext().Log($"==============================================================");
            AppContext.GetAppDataContext().Log($"开始部署{environment.Host}:{environment.Port}");

            selectedEnvironments.First(it => it.Host == environment.Host).Status = "开始部署";


            Stopwatch sw = new();
            sw.Start();
            var currentFileInfos = FileHelper.GetFileInfos(deployParams.TargetPath);
            AppContext.GetAppDataContext().Log($"获取文件信息耗时:{sw.ElapsedMilliseconds}ms");
            sw.Restart();

            if (AppContext.GetAppDataContext().StopToken?.IsCancellationRequested == true) return false;

            calculateNeedDeployFiles ??=await  agentHttpService.Compare(currentFileInfos, deployParams.ServiceName,
                AppContext.GetAppDataContext().StopToken?.Token??default);
            AppContext.GetAppDataContext().Log($"对比文件信息耗时:{sw.ElapsedMilliseconds}ms");
            
            sw.Restart();
            
            AppContext.GetAppDataContext()
                .Log($"需要复制文件:{string.Join(",", calculateNeedDeployFiles?.Select(it => $"【{it.FileName}】") ?? Array.Empty<string>())}");

            if (calculateNeedDeployFiles.IsNullOrEmpty())
            {
                AppContext.GetAppDataContext().Log($"无需部署{environment.Host}:{environment.Port}");

                selectedEnvironments.First(it => it.Host == environment.Host).Status = "部署完成";
                return false;
            }

            if (AppContext.GetAppDataContext().StopToken?.IsCancellationRequested == true) return false;
            sw.Restart();
            
            memoryStream ??= await Task.Run(() => CreateZipFile(calculateNeedDeployFiles!));
            
            AppContext.GetAppDataContext().Log($"打包文件耗时:{sw.ElapsedMilliseconds}ms");
            if (AppContext.GetAppDataContext().StopToken?.IsCancellationRequested == true) return false;

            selectedEnvironments.First(it => it.Host == environment.Host).Status = "文件比较完毕";

            try
            {
                var result=await agentHttpService.Upload(memoryStream, deployParams.ServiceName, calculateNeedDeployFiles);
                if(!result)
                    return false;
            }
            catch (Exception e)
            {
                AppContext.GetAppDataContext().Log($"部署失败{environment.Host}:{environment.Port}");
                AppContext.GetAppDataContext().Log($"{e.Message}");
                AppContext.GetAppDataContext().Log($"{e.StackTrace}");
                return false;
            }

            AppContext.GetAppDataContext().Log($"部署完成{environment.Host}:{environment.Port}");
            if (AppContext.GetAppDataContext().StopToken?.IsCancellationRequested == true) return false;

            selectedEnvironments.First(it => it.Host == environment.Host).Status = "部署完成";

        }

        if (deployParams.EnableNotify&&!string.IsNullOrWhiteSpace(AppContext.GetAppDataContext().GlobalSetting.QiyeWeChatKey))
        {
            var description = AppContext.GetAppDataContext().Services.First(it => it.Name == deployParams.ServiceName).Description;
            if (string.IsNullOrWhiteSpace(description))
                return true;
            string msg = $"# LightDeploy部署 \n" +
                              $"{description} \n" +
                              $"# 发布说明 \n" +
                              $"{deployParams.Remark} \n"+
                              $"# 发布时间 \n" +
                              $"{DateTime.Now:yyyy-MM-dd hh:mm:ss}";;
            
            QiyeWeChatNotifyDto qiyeWeChatNotifyDto=new()
            {
                markdown = new()
                {
                    content = msg
                }
            };
            var resp=await $"https://qyapi.weixin.qq.com//cgi-bin/webhook/send?key={AppContext.GetAppDataContext().GlobalSetting.QiyeWeChatKey}"
                .PostJsonAsync(qiyeWeChatNotifyDto)
                .ReceiveString();
            AppContext.Log($"企业微信通知完成 {resp}");
        }
        return true;
    }

    private static async Task ConnectSignalR(string url)
    {
        connection = new HubConnectionBuilder()
            .WithUrl(url)
            .WithAutomaticReconnect()
            .Build();
        connection.On("Log", (string msg) => { AppContext.GetAppDataContext().Log($"Agent消息: " + msg); });
        await connection.StartAsync();
    }

    private static async Task DisConnectSignalR()
    {
        if (connection != null)
        {
            await connection.StopAsync();
            connection = null;
        }
    }

    private static MemoryStream CreateZipFile(List<FileInfoDto> calculateNeedDeployFiles)
    {
        var zipPath = AppDomain.CurrentDomain.BaseDirectory;
        var fileInfos = calculateNeedDeployFiles.Select(it => (Path.Combine(it.AbsoluteDirectory, it.FileName),
            Path.Combine(it.RelativeDirectory, it.FileName)));
        var memoryStream = FileHelper.CompressFiles(fileInfos.ToList());
        return memoryStream;
    }

    private static List<FileInfoDto> CalculateNeedDeployFiles(List<FileInfoDto> currentFileInfos,
        List<FileInfoDto> remoteFiles)
    {
        List<FileInfoDto> result = new();
        foreach (var currentFileInfo in currentFileInfos)
        {
            var exist = remoteFiles.FirstOrDefault(it =>
                it.FileName == currentFileInfo.FileName && it.RelativeDirectory == currentFileInfo.RelativeDirectory
                                                        && it.FileSize == currentFileInfo.FileSize);
            if (exist is null)
            {
                result.Add(currentFileInfo);
            }
        }

        return result;
    }

    public static async Task Install(
        (string ServiceName, string ServiceExe, string ServiceParams, string ServiceDescription) serviceParams,
        DeployParams deployParams)
    {
        var tmpDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tmp", Guid.NewGuid().ToString("N"));
        try
        {
            if (deployParams.BuildMode == 0)
            {
                var isSelfContained = deployParams.IsSelfContained ? "--self-contained" : string.Empty;
                await ProcessorHelper.InvokeAsync("dotnet",
                    $" publish {deployParams.TargetPath} -c Release -o {tmpDir} {isSelfContained} ", true);
                AppContext.GetAppDataContext().Log("编译完成");
                deployParams.TargetPath = tmpDir;
            }
           
            var selectedEnvironments = AppContext.GetAppDataContext().SelectedEnvironments
                .Where(it => it.NeedDeploy).ToList();
            MemoryStream memoryStream = null;
            AppContext.GetAppDataContext().Log("开始部署");

            foreach (var environment in selectedEnvironments)
            {
                await ConnectSignalR($"http://{environment.Host}:{environment.Port}/agent");
                if (AppContext.GetAppDataContext().StopToken!.IsCancellationRequested) return ;
                AppContext.GetAppDataContext().Log($"==============================================================");
                AppContext.GetAppDataContext().Log($"开始部署{environment.Host}:{environment.Port}");

                selectedEnvironments.First(it => it.Host == environment.Host).Status = "开始部署";


                Stopwatch sw = new();
                sw.Start();
                var currentFileInfos = FileHelper.GetFileInfos(deployParams.TargetPath);
                
                AppContext.GetAppDataContext().Log($"获取文件信息耗时:{sw.ElapsedMilliseconds}ms");
                
                sw.Restart();

                if (AppContext.GetAppDataContext().StopToken?.IsCancellationRequested == true) return ;


                if (AppContext.GetAppDataContext().StopToken?.IsCancellationRequested == true) return ;
                sw.Restart();
                if (memoryStream == null)
                    memoryStream =
                        await Task.Run(() => CreateZipFile(currentFileInfos));
                AppContext.GetAppDataContext().Log($"打包文件耗时:{sw.ElapsedMilliseconds}ms");
                if (AppContext.GetAppDataContext().StopToken?.IsCancellationRequested == true) return ;

                selectedEnvironments.First(it => it.Host == environment.Host).Status = "文件比较完毕";

                try
                {
                    AppContext.GetAppDataContext().Log($"开始通知Agent发布文件");

                    var response = await $"http://{environment.Host}:{environment.Port}/api/deploy/installwindowsservice"
                        .WithHeader("Authorization", environment.AuthKey ?? "")
                        .PostMultipartAsync(mp =>
                        {
                            mp.AddFile("File", new MemoryStream(memoryStream.ToArray()), "file.zip");
                            mp.AddString("ServiceName", deployParams.ServiceName);
                            mp.AddString("ConnectionId", connection?.ConnectionId ?? "");
                            mp.AddString("Params", serviceParams.ServiceParams ?? "");
                            mp.AddString("ExeFullPath", serviceParams.ServiceExe ?? "");
                            mp.AddString("ServiceDescription", serviceParams.ServiceDescription ?? "");
                        });

                    if (response.StatusCode != 200)
                    {
                        AppContext.GetAppDataContext().Log($"部署失败{environment.Host}:{environment.Port}");

                        continue;
                    }
                }
                catch (FlurlHttpException e)
                {
                    var body = await e.GetResponseStringAsync();
                    AppContext.GetAppDataContext().Log($"部署失败{environment.Host}:{environment.Port}");
                    AppContext.GetAppDataContext().Log($"返回消息 {e.Message}");
                    AppContext.GetAppDataContext().Log($"返回消息 {body}");
                    return ;
                }

                AppContext.GetAppDataContext().Log($"部署完成{environment.Host}:{environment.Port}");
                
                if (AppContext.GetAppDataContext().StopToken?.IsCancellationRequested == true) return ;

                selectedEnvironments.First(it => it.Host == environment.Host).Status = "部署完成";

                await DisConnectSignalR();
            }

        }
        finally
        {
            if(Directory.Exists(tmpDir))
                Directory.Delete(tmpDir, true);
        }
    }

    public static async Task StartService(DeployParams deployParams)
    {
        var selectedEnvironments = AppContext.GetAppDataContext().SelectedEnvironments
            .Where(it => it.NeedDeploy).ToList();
        foreach (var environment in selectedEnvironments)
        {
            try
            {
                await using var agentService = new AgentHttpService(environment);
                await agentService.StartService(deployParams.ServiceName);
            }
            catch (Exception e)
            {
                AppContext.Log(e.Message);
            }
           
        }
    }

    public static async Task StopService(DeployParams deployParams)
    {
        var selectedEnvironments = AppContext.GetAppDataContext().SelectedEnvironments
            .Where(it => it.NeedDeploy).ToList();
        foreach (var environment in selectedEnvironments)
        {
            try
            {
                await using var agentService = new AgentHttpService(environment);
                await agentService.StopService(deployParams.ServiceName);
            }
            catch (Exception e)
            {
                AppContext.Log(e.Message);
            }
           
        }
    }

    /// <summary>
    /// 刷新当前选择服务对应环境的服务运行状态
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public static async Task RefreshSelectEnvironmentsStatus(string serviceName)
    {
        foreach (var environment in AppContext.GetAppDataContext().SelectedEnvironments)
        {
            try
            {
                await using var agentService = new AgentHttpService(environment);
                var data=await agentService.RefreshService(serviceName);
                environment.ServiceStatus = data;
            }
            catch (Exception e)
            {
                AppContext.Log($"【{environment.Host}】获取服务状态失败: "+e.Message);
            }
          
        }
    }
}