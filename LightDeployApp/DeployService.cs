using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Flurl;
using Flurl.Http;
using LightDeploy.ClientAgent.Dto;
using LightDeployApp.Tables;
using SevenZipExtractor;
using SqlSugar;
using SW.Core.Helper;

namespace LightDeployApp;

public class DeployService
{
    public static async Task Deploy(TextBox textBox, DeployParams deployParams)
    {
        try
        {
            if(deployParams.BuildMode==0)
                await DeployProject(textBox,deployParams);
            else
                await DeployFolder(textBox,deployParams);
        }
        catch (Exception e)
        {
            textBox.Text += e.Message+"\n";
        }
       
    }

    private static async Task DeployProject(TextBox textBox, DeployParams deployParams)
    {
        var tmpDir=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"tmp",Guid.NewGuid().ToString("N"));
        var isSelfContained = deployParams.IsSelfContained?"--self-contained":string.Empty;
        await ProcessorHelper.InvokeAsync("dotnet", $" publish {deployParams.TargetPath} -c Release -o {tmpDir} {isSelfContained} ", true, textBox);
        textBox.Text+="编译完成\n";
        try
        {
            deployParams.TargetPath = tmpDir;
            await DeployToServer(textBox,deployParams);
        }
        finally
        {
            Directory.Delete(tmpDir,true);
        }
    }

    private static async Task DeployFolder(TextBox textBox, DeployParams deployParams)
    {
        await DeployToServer(textBox,deployParams);
    }
    
    private static async Task DeployToServer(TextBox textBox, DeployParams deployParams)
    {
        var selectedEnvironments = AppContext.GetAppDataContext().SelectedEnvironments;
        var environments = DBHelper.GetClient().Queryable<TEnvironment>().Where(it => it.Name == deployParams.Environment).ToList();
        List<FileInfoDto> calculateNeedDeployFiles = null;
        foreach (var environment in environments)
        {
            textBox.Text+=$"开始部署{environment.Host}:{environment.Port}\n";
            selectedEnvironments.First(it => it.Host == environment.Host).Status = "开始部署";

          
            var currentFileInfos=FileHelper.GetFileInfos(deployParams.TargetPath);
            
            calculateNeedDeployFiles ??= await $"http://{environment.Host}:{environment.Port}/api/deploy/compare"
                .SetQueryParam("serviceName", deployParams.ServiceName)
                .PostJsonAsync(currentFileInfos)
                .ReceiveJson<List<FileInfoDto>>();
            
            // var calculateNeedDeployFiles = CalculateNeedDeployFiles(currentFileInfos, remoteFiles);
            textBox.Text += $"需要复制文件:{string.Join(",", calculateNeedDeployFiles.Select(it => it.FileName))}\n";
            if(calculateNeedDeployFiles.Count==0)
            {
                textBox.Text+=$"无需部署{environment.Host}:{environment.Port}\n";
                continue;
            }
            
            var memoryStream =
                await Task.Run(() => CreateZipFile(calculateNeedDeployFiles));
            
            selectedEnvironments.First(it => it.Host == environment.Host).Status = "文件比较完毕";

            try
            {
                var response = await $"http://{environment.Host}:{environment.Port}/api/deploy/deploy".PostMultipartAsync(mp =>
                {
                    mp.AddFile("File", memoryStream, "file.zip");
                    mp.AddString("ServiceName", deployParams.ServiceName);
                });
                 
                if (response.StatusCode != 200)
                {
                    textBox.Text+=$"部署失败{environment.Host}:{environment.Port}\n";
                    continue;
                }
            }
            catch (FlurlHttpException e)
            {
                var body=await e.GetResponseStringAsync();
                textBox.Text+=$"部署失败{environment.Host}:{environment.Port}\n";
                textBox.Text+=$"返回消息 {e.Message}\n";
                textBox.Text+=$"返回消息 {body}\n";
                continue;
            }
         
            textBox.Text+=$"部署完成{environment.Host}:{environment.Port}\n";
            
            selectedEnvironments.First(it => it.Host == environment.Host).Status = "部署完成";
            
            if (deployParams.EnableHealthCheck)
            {
                int count = 0;
                while (count<10&&!string.IsNullOrWhiteSpace(environment.HealthCheckUrl))
                {
                    try
                    {
                        var result=await environment.HealthCheckUrl.WithTimeout(3).GetAsync();
                        result.ResponseMessage.EnsureSuccessStatusCode();
                        break;
                    }
                    catch (Exception)
                    {
                        textBox.Text+=$"{environment.HealthCheckUrl}健康检查失败{count+1}次,暂停5秒 (最多10次)\n";
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