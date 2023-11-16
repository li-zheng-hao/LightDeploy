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
using LightDeployApp.Tables;
using LightDeployApp.Windows;
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
        List<FileInfoDto> calculateNeedDeployFiles = null;
        MemoryStream memoryStream = null;
        foreach (var environment in selectedEnvironments)
        {
            await ConnectSignalR($"http://{environment.Host}:{environment.Port}/agent");
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

            calculateNeedDeployFiles ??= await $"http://{environment.Host}:{environment.Port}/api/deploy/compare"
                .WithHeader("Authorization", environment.AuthKey ?? "")
                .SetQueryParam("serviceName", deployParams.ServiceName)
                .WithHeader("Authorization", environment.AuthKey ?? "")
                .PostJsonAsync(currentFileInfos, cancellationToken: AppContext.GetAppDataContext().StopToken!.Token)
                .ReceiveJson<List<FileInfoDto>>();
            AppContext.GetAppDataContext().Log($"对比文件信息耗时:{sw.ElapsedMilliseconds}ms");
            sw.Restart();
            // var calculateNeedDeployFiles = CalculateNeedDeployFiles(currentFileInfos, remoteFiles);
            AppContext.GetAppDataContext()
                .Log($"需要复制文件:{string.Join(",", calculateNeedDeployFiles.Select(it => $"【{it.FileName}】"))}");

            if (calculateNeedDeployFiles.Count == 0)
            {
                AppContext.GetAppDataContext().Log($"无需部署{environment.Host}:{environment.Port}");

                selectedEnvironments.First(it => it.Host == environment.Host).Status = "部署完成";
                return false;
            }

            if (AppContext.GetAppDataContext().StopToken?.IsCancellationRequested == true) return false;
            sw.Restart();
            if (memoryStream == null)
                memoryStream =
                    await Task.Run(() => CreateZipFile(calculateNeedDeployFiles));
            AppContext.GetAppDataContext().Log($"打包文件耗时:{sw.ElapsedMilliseconds}ms");
            if (AppContext.GetAppDataContext().StopToken?.IsCancellationRequested == true) return false;

            selectedEnvironments.First(it => it.Host == environment.Host).Status = "文件比较完毕";

            try
            {
                var response = await $"http://{environment.Host}:{environment.Port}/api/deploy/deploy"
                    .WithHeader("Authorization", environment.AuthKey ?? "")
                    .PostMultipartAsync(mp =>
                    {
                        mp.AddFile("File", new MemoryStream(memoryStream.ToArray()), "file.zip");
                        mp.AddString("ServiceName", deployParams.ServiceName);
                        mp.AddJson("FileInfos", calculateNeedDeployFiles);
                        mp.AddString("ConnectionId", connection?.ConnectionId ?? "");
                        mp.AddString("HealthCheckUrl", environment.HealthCheckUrl ?? "");
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
                return false;
            }

            AppContext.GetAppDataContext().Log($"部署完成{environment.Host}:{environment.Port}");
            if (AppContext.GetAppDataContext().StopToken?.IsCancellationRequested == true) return false;

            selectedEnvironments.First(it => it.Host == environment.Host).Status = "部署完成";

            await DisConnectSignalR();
        }

        if (deployParams.EnableNotify&&!string.IsNullOrWhiteSpace(AppContext.GetAppDataContext().GlobalSetting.QiyeWeChatKey))
        {
            var description = AppContext.GetAppDataContext().Services.First(it => it.Name == deployParams.ServiceName).Description;
            if (string.IsNullOrWhiteSpace(description))
                return true;
            string msg = $"# LightDeploy部署 \n" +
                              $"{description} \n" +
                              $"# 发布说明 \n" +
                              $"{deployParams.Remark}";
            
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
                var response = await $"http://{environment.Host}:{environment.Port}/api/deploy/startservice"
                    .WithHeader("Authorization", environment.AuthKey ?? "")
                    .SetQueryParam("serviceName", deployParams.ServiceName)
                    .PostAsync();
            }
            catch (FlurlHttpException e)
            {
                var body = await e.GetResponseStringAsync();
                AppContext.GetAppDataContext().Log($"失败{environment.Host}:{environment.Port}");
                AppContext.GetAppDataContext().Log($"返回消息 {e.Message}");
                AppContext.GetAppDataContext().Log($"返回消息 {body}");
                return ;
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
                var response = await $"http://{environment.Host}:{environment.Port}/api/deploy/stopservice"
                    .WithHeader("Authorization", environment.AuthKey ?? "")
                    .SetQueryParam("serviceName", deployParams.ServiceName)
                    .PostAsync();
            }
            catch (FlurlHttpException e)
            {
                var body = await e.GetResponseStringAsync();
                AppContext.GetAppDataContext().Log($"失败{environment.Host}:{environment.Port}");
                AppContext.GetAppDataContext().Log($"返回消息 {e.Message}");
                AppContext.GetAppDataContext().Log($"返回消息 {body}");
                return ;
            }
                
        }
    }
}