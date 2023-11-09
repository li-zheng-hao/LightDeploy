using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Flurl;
using Flurl.Http;
using LightDeploy.ClientAgent.Dto;
using LightDeployApp.Dtos;
using LightDeployApp.Tables;
using SevenZipExtractor;
using SqlSugar;
using SW.Core.Helper;

namespace LightDeployApp;

public class DeployService
{
    public static async Task Deploy(DeployParams deployParams)
    {
        try
        {
            if(deployParams.BuildMode==0)
                await DeployProject(deployParams);
            else
                await DeployFolder(deployParams);
        }
        catch (Exception e)
        {
            AppContext.GetAppDataContext().Log(e.Message);
        }
       
    }

    private static async Task DeployProject( DeployParams deployParams)
    {
        var tmpDir=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"tmp",Guid.NewGuid().ToString("N"));
        var isSelfContained = deployParams.IsSelfContained?"--self-contained":string.Empty;
        await ProcessorHelper.InvokeAsync("dotnet", $" publish {deployParams.TargetPath} -c Release -o {tmpDir} {isSelfContained} ", true);
        await Task.Delay(2000);
        AppContext.GetAppDataContext().Log("编译完成");
        try
        {
            deployParams.TargetPath = tmpDir;
            await DeployToServer(deployParams);
        }
        finally
        {
            Directory.Delete(tmpDir,true);
        }
    }

    private static async Task DeployFolder( DeployParams deployParams)
    {
        await DeployToServer(deployParams);
    }
    
    private static async Task DeployToServer(DeployParams deployParams)
    {
        var selectedEnvironments = AppContext.GetAppDataContext().SelectedEnvironments
            .Where(it=>it.NeedDeploy).ToList();
        List<FileInfoDto> calculateNeedDeployFiles = null;
        MemoryStream memoryStream = null;
        foreach (var environment in selectedEnvironments)
        {
            if (AppContext.GetAppDataContext().StopToken!.IsCancellationRequested) return;
            AppContext.GetAppDataContext().Log($"==============================================================");
            AppContext.GetAppDataContext().Log($"开始部署{environment.Host}:{environment.Port}");

            selectedEnvironments.First(it => it.Host == environment.Host).Status = "开始部署";

          
            var currentFileInfos=FileHelper.GetFileInfos(deployParams.TargetPath);
            if (AppContext.GetAppDataContext().StopToken?.IsCancellationRequested==true) return;

            calculateNeedDeployFiles ??= await $"http://{environment.Host}:{environment.Port}/api/deploy/compare"
                .WithHeader("Authorization", environment.AuthKey??"")
                .SetQueryParam("serviceName", deployParams.ServiceName)
                .WithHeader("Authorization", environment.AuthKey??"")
                .PostJsonAsync(currentFileInfos,cancellationToken:AppContext.GetAppDataContext().StopToken!.Token)
                .ReceiveJson<List<FileInfoDto>>();
            
            // var calculateNeedDeployFiles = CalculateNeedDeployFiles(currentFileInfos, remoteFiles);
            AppContext.GetAppDataContext().Log($"需要复制文件:{string.Join(",", calculateNeedDeployFiles.Select(it => it.FileName))}");

            if(calculateNeedDeployFiles.Count==0)
            {
                AppContext.GetAppDataContext().Log($"无需部署{environment.Host}:{environment.Port}");

                selectedEnvironments.First(it => it.Host == environment.Host).Status = "部署完成";
                continue;
            }
            if (AppContext.GetAppDataContext().StopToken?.IsCancellationRequested==true) return;

            if(memoryStream==null)
                memoryStream =
                    await Task.Run(() => CreateZipFile(calculateNeedDeployFiles));
            if (AppContext.GetAppDataContext().StopToken?.IsCancellationRequested==true) return;

            selectedEnvironments.First(it => it.Host == environment.Host).Status = "文件比较完毕";

            try
            {
               
                var response = await $"http://{environment.Host}:{environment.Port}/api/deploy/deploy"
                    .WithHeader("Authorization", environment.AuthKey??"")
                    .PostMultipartAsync(mp =>
                {
                    mp.AddFile("File", new MemoryStream(memoryStream.ToArray()), "file.zip");
                    mp.AddString("ServiceName", deployParams.ServiceName);
                });
                 
                if (response.StatusCode != 200)
                {
                    AppContext.GetAppDataContext().Log($"部署失败{environment.Host}:{environment.Port}");

                    continue;
                }
            }
            catch (FlurlHttpException e)
            {
                var body=await e.GetResponseStringAsync();
                AppContext.GetAppDataContext().Log($"部署失败{environment.Host}:{environment.Port}");
                AppContext.GetAppDataContext().Log($"返回消息 {e.Message}");
                AppContext.GetAppDataContext().Log($"返回消息 {body}");
                continue;
            }
         
            AppContext.GetAppDataContext().Log($"部署完成{environment.Host}:{environment.Port}");
            if (AppContext.GetAppDataContext().StopToken?.IsCancellationRequested==true) return;

            selectedEnvironments.First(it => it.Host == environment.Host).Status = "部署完成";
            
            if (deployParams.EnableHealthCheck)
            {
                int count = 0;
                while (count<10&&!string.IsNullOrWhiteSpace(environment.HealthCheckUrl))
                {
                    if (AppContext.GetAppDataContext().StopToken?.IsCancellationRequested==true) return;

                    try
                    {
                        var result=await environment.HealthCheckUrl.WithTimeout(3).GetAsync();
                        result.ResponseMessage.EnsureSuccessStatusCode();
                        break;
                    }
                    catch (Exception)
                    {
                        AppContext.GetAppDataContext().Log($"{environment.HealthCheckUrl}健康检查失败{count+1}次,暂停5秒 (最多10次)");
                        await Task.Delay(3000);
                        count++;
                    }
                }
                selectedEnvironments.First(it => it.Host == environment.Host).Status = "健康检查通过";
            }

        }
    }

    private static MemoryStream CreateZipFile(List<FileInfoDto> calculateNeedDeployFiles)
    {
        var zipPath = AppDomain.CurrentDomain.BaseDirectory;
        var fileInfos=calculateNeedDeployFiles.Select(it => (Path.Combine(it.AbsoluteDirectory, it.FileName),Path.Combine(it.RelativeDirectory, it.FileName)));
        var memoryStream=FileHelper.CompressFiles(fileInfos.ToList());
        return memoryStream;
    }

    private static List<FileInfoDto> CalculateNeedDeployFiles(List<FileInfoDto> currentFileInfos, List<FileInfoDto> remoteFiles)
    {
        List<FileInfoDto> result = new();
        foreach (var currentFileInfo in currentFileInfos)
        {
            var exist=remoteFiles.FirstOrDefault(it =>
                it.FileName == currentFileInfo.FileName && it.RelativeDirectory == currentFileInfo.RelativeDirectory
                &&it.FileSize==currentFileInfo.FileSize);
            if (exist is null)
            {
                result.Add(currentFileInfo);
            }
        }

        return result;
    }
}