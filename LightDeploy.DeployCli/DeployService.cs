using Flurl.Http;
using LightDeploy.ClientAgent.Dto;
using LightDeploy.DeployCli;
using LightDeploy.DeployCli.Tables;

namespace LightDeployApp;

public class DeployService
{
    public static async Task Deploy(DeployParams deployParams)
    {
        await DeployFolder(deployParams);
    }

    private static async Task DeployFolder( DeployParams deployParams)
    {
        await DeployToServer(deployParams);
    }
    
    private static async Task DeployToServer( DeployParams deployParams)
    {
        var environments = DBHelper.GetClient().Queryable<TEnvironment>().Where(it => it.Name == deployParams.Environment).ToList();
        List<FileInfoDto> calculateNeedDeployFiles = null;
        MemoryStream memoryStream = null;
        foreach (var environment in environments)
        {
            Console.WriteLine($"==============================================================");
            Console.WriteLine($"开始部署{environment.Host}:{environment.Port}");
          
            var currentFileInfos=FileHelper.GetFileInfos(deployParams.TargetPath);
            
            calculateNeedDeployFiles ??= await $"http://{environment.Host}:{environment.Port}/api/deploy/compare"
                .WithHeader("Authorization", environment.AuthKey??"")
                .SetQueryParam("serviceName", deployParams.ServiceName)
                .WithHeader("Authorization", environment.AuthKey??"")
                .PostJsonAsync(currentFileInfos)
                .ReceiveJson<List<FileInfoDto>>();
            
            // var calculateNeedDeployFiles = CalculateNeedDeployFiles(currentFileInfos, remoteFiles);
            Console.WriteLine($"需要复制文件:{string.Join(",", calculateNeedDeployFiles.Select(it => it.FileName))}");

            if(calculateNeedDeployFiles.Count==0)
            {
                Console.WriteLine($"无需部署{environment.Host}:{environment.Port}");

                continue;
            }
            if(memoryStream==null)
                memoryStream =
                    await Task.Run(() => CreateZipFile(calculateNeedDeployFiles));
            
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
                    Console.Error.WriteLine($"部署失败{environment.Host}:{environment.Port}");

                    continue;
                }
            }
            catch (FlurlHttpException e)
            {
                var body=await e.GetResponseStringAsync();
                Console.Error.WriteLine($"部署失败{environment.Host}:{environment.Port}");
                Console.Error.WriteLine($"返回消息 {e.Message}");
                Console.Error.WriteLine($"返回消息 {body}");
                continue;
            }
         
            Console.WriteLine($"部署完成{environment.Host}:{environment.Port}");
            
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
                        Console.WriteLine($"{environment.HealthCheckUrl}健康检查失败{count+1}次,暂停5秒 (最多10次)");
                        await Task.Delay(3000);
                        count++;
                    }
                }
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