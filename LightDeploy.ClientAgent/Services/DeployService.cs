using System.Management;
using System.Security.Cryptography;
using System.ServiceProcess;
using System.Text.Json;
using Flurl.Http;
using LightDeploy.ClientAgent.Dto;
using LightDeploy.ClientAgent.Hubs;
using Microsoft.AspNetCore.SignalR;
using Polly;
using SevenZipExtractor;

namespace LightDeploy.ClientAgent.Services;

public class DeployService
{
    private readonly ILogger<DeployService> _logger;
    private readonly IHubContext<DeployHub> _hub;
    private string _connectionId;

    public DeployService(ILogger<DeployService> logger,IHubContext<DeployHub> hub)
    {
        _logger = logger;
        _hub = hub;
    }
    public async Task Deploy(DeployDto deployDto)
    {
        _connectionId=deployDto.ConnectionId;
        var subDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tmp", Guid.NewGuid().ToString("N"));
        var backupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tmp", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(subDir);
        Directory.CreateDirectory(backupDir);
        string exeDir = string.Empty;
        try
        {
            var readStream = deployDto.File.OpenReadStream();
            using ArchiveFile archiveFile = new ArchiveFile(readStream);
            archiveFile.Extract(subDir); // extract all
            await Log("解压完成");
            // 获取当前系统服务
            var services = ServiceController.GetServices();
            var service = services.FirstOrDefault(it => it.ServiceName == deployDto.ServiceName);
            if (service is null)
            {
                await Log("服务不存在");
                throw new BusinessException("服务不存在");

            }

            await Log("正在停止服务...");

            WindowsServiceHelper.StopService(deployDto.ServiceName);
            await Log("停止服务完成");

            string exePath = WindowsServiceHelper.GetWindowsServiceLocation(deployDto.ServiceName);
            if (exePath == string.Empty)
                throw new BusinessException("服务路径不存在");

            _logger.LogInformation($"服务路径:{exePath}");
            exeDir = Path.GetDirectoryName(exePath)!;

            await Log("开始备份文件");

            await Backup(exeDir, backupDir, JsonSerializer.Deserialize<List<FileInfoDto>>(deployDto.FileInfos));

            Polly.Retry.RetryPolicy retryPolicy =
                Policy.Handle<Exception>().WaitAndRetry(3, i => TimeSpan.FromSeconds(2));
            retryPolicy.Execute(() => { CopyFilesRecursively(subDir, exeDir); });

            WindowsServiceHelper.StartService(deployDto.ServiceName);
            
            await Log("启动服务完成");

            if (!string.IsNullOrWhiteSpace(deployDto.HealthCheckUrl))
            {
                await Log("开始健康检查");

                // 健康检查
                var result = await HealthCheck(deployDto.HealthCheckUrl);
                if (result == false)
                {
                    await Log("健康检查未通过,开始回滚");
                    WindowsServiceHelper.StopService(deployDto.ServiceName, 30000);
                    Restore(exeDir, backupDir);
                    WindowsServiceHelper.StartService(deployDto.ServiceName, 30000);
                    await Log("健康检查回滚完成,请手动检查服务是否正常");
                    throw new BusinessException("健康检查未通过");
                }
                await Log("健康检查完成");
            }

        }
        catch (Exception e)
        {
            await Log("发布出现未处理异常" + e.Message);
            _logger.LogError(e,e.Message);
            throw;
        }
        finally
        {
            Directory.Delete(subDir,true);
            Directory.Delete(backupDir,true);
        }
        
    }

    private async Task<bool> HealthCheck(string deployDtoHealthCheckUrl)
    {
        for (int i = 0; i < 10; i++)
        {
            try
            {
                var response=await deployDtoHealthCheckUrl.AllowAnyHttpStatus().WithTimeout(2).GetAsync();
                if (response.ResponseMessage.IsSuccessStatusCode)
                {
                    await Log("健康检查通过,服务已启动");
                    return true;
                }
            }
            catch
            {
                // ignored
            }

            await Log($"健康检查失败,休息3秒再检查 第{i+1}次/10");

            await Task.Delay(3000);
        }

        return false;
    }

    private void Restore(string exeDir, string backupDir)
    {
        if (string.IsNullOrWhiteSpace(exeDir) || string.IsNullOrWhiteSpace(backupDir))
            return;
        if(!Directory.Exists(backupDir))
            return;
        if (!Directory.Exists(exeDir))
            return;
        CopyFilesRecursively(backupDir, exeDir);
    }

    private async Task Log(string message)
    {
        await _hub.Clients.Clients(_connectionId).SendAsync("Log", message);

    }

    /// <summary>
    /// 备份文件
    /// </summary>
    /// <param name="exeDir"></param>
    /// <param name="backupDir"></param>
    /// <param name="deployDtoFileInfos"></param>
    /// <exception cref="NotImplementedException"></exception>
    private async Task Backup(string exeDir, string backupDir, List<FileInfoDto> deployDtoFileInfos)
    {
        await Log($"备份文件夹 {backupDir}");
        foreach (var deployDtoFileInfo in deployDtoFileInfos)
        {
            var filePath=Path.Combine(exeDir, deployDtoFileInfo.RelativeDirectory, deployDtoFileInfo.FileName);
            var targetDir=Path.Combine(backupDir, deployDtoFileInfo.RelativeDirectory);
            if (File.Exists(filePath))
            {
                Directory.CreateDirectory(targetDir);
                await Log($"备份文件: {Path.Combine(targetDir, deployDtoFileInfo.FileName)}");
                File.Copy(filePath, Path.Combine(targetDir, deployDtoFileInfo.FileName));
            }
        }
    }

    /// <summary>
    /// 比较两个目录以及子目录下所有文件
    /// </summary>
    /// <param name="sourceDir"></param>
    /// <param name="targetDir"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void CopyFilesRecursively(string sourceDir, string targetDir)
    {
        var sourceDirInfo = new DirectoryInfo(sourceDir);
        var exeDirInfo = new DirectoryInfo(targetDir);

        foreach (var directoryInfo in sourceDirInfo.GetDirectories())
        {
            var subdir = exeDirInfo.GetDirectories().FirstOrDefault(it => it.Name == directoryInfo.Name);
            if (subdir is null)
            {
                subdir=Directory.CreateDirectory(Path.Combine(targetDir, directoryInfo.Name));
            }
            CopyFilesRecursively(directoryInfo.FullName, subdir.FullName);
        }

        
        foreach (var sourceFile in sourceDirInfo.GetFiles())
        {
            var exeFile = exeDirInfo.GetFiles().FirstOrDefault(it => it.Name == sourceFile.Name);
            if (exeFile is null)
            {
                File.Copy(sourceFile.FullName, Path.Combine(targetDir, sourceFile.Name));
                Log($"复制文件到{Path.Combine(targetDir, sourceFile.Name)}").Wait();
                _logger.LogInformation($"复制文件到{Path.Combine(targetDir, sourceFile.Name)}");
            }
            else
            {
               
                File.Copy(sourceFile.FullName, Path.Combine(targetDir, sourceFile.Name), true);
                _logger.LogInformation($"复制文件到{Path.Combine(targetDir, sourceFile.Name)}");
                Log($"复制文件到{Path.Combine(targetDir, sourceFile.Name)}").Wait();
            }
        }
    }

    public List<FileInfoDto> GetFileInfos(string serviceName,string ignoreFileExtensions)
    {
        string[] ignoreArr = new[] { ".log", ".db", ".db-shm", ".db-wal" };
        if (!string.IsNullOrWhiteSpace(ignoreFileExtensions))
        {
            ignoreArr = ignoreFileExtensions.Split(",");
        }
        string exePath=WindowsServiceHelper.GetWindowsServiceLocation(serviceName);

        if(exePath == string.Empty)
            throw new BusinessException("服务路径不存在");
            
        string exeDir=Path.GetDirectoryName(exePath)!;
        var exeDirInfo = new DirectoryInfo(exeDir);
        var fileInfos = exeDirInfo.GetFiles("*.*", SearchOption.AllDirectories);
        var fileInfoDtos = fileInfos.Select(it => new FileInfoDto()
        {
            FileName = it.Name,
            RelativeDirectory = Path.GetDirectoryName(it.FullName)!.Replace(exeDir, string.Empty),
            FileSize = it.Length,
            AbsoluteDirectory = Path.GetDirectoryName(it.FullName),
            LastWriteTime = it.LastWriteTime,
            MD5=GetFileMd5(it.FullName,ignoreArr)
        }).ToList();
        foreach (var fileInfoDto in fileInfoDtos.Where(it=>it.RelativeDirectory.StartsWith("/")||it.RelativeDirectory.StartsWith("\\")))
        {
            fileInfoDto.RelativeDirectory=fileInfoDto.RelativeDirectory.Substring(1);
        }
        return fileInfoDtos;
    }

    public List<FileInfoDto> CompareFileInfos(string serviceName, List<FileInfoDto> fileInfoDtos)
    {
        string exePath=WindowsServiceHelper.GetWindowsServiceLocation(serviceName);

        if(exePath == string.Empty)
            throw new BusinessException("服务路径不存在");
            
        string exeDir=Path.GetDirectoryName(exePath)!;
        List<FileInfoDto> result = new();
        foreach (var fileInfoDto in fileInfoDtos)
        {
            var filePath=Path.Combine(exeDir, fileInfoDto.RelativeDirectory, fileInfoDto.FileName);
            if (!File.Exists(filePath))
            {
                result.Add(fileInfoDto);
            }
            else
            {
                FileInfo fileInfo=new FileInfo(filePath);
                if (fileInfo.Length!=fileInfoDto.FileSize||fileInfo.LastWriteTime<fileInfoDto.LastWriteTime)
                {
                    result.Add(fileInfoDto);
                }                
                // var md5=GetFileMd5(filePath, ".log", ".db", ".db-shm", ".db-wal");
                // if (md5!=fileInfoDto.MD5 )
                // {
                    // result.Add(fileInfoDto);
                // }
            }
        }
        return result;

    }
    /// <summary>
    /// 计算文件MD5
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetFileMd5(string path,params string[] ignoreExtensions)
    {
       
        if(ignoreExtensions.Contains( Path.GetExtension(path),StringComparer.OrdinalIgnoreCase))
            return string.Empty;
        try
        {
            using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(fileStream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
           
        }
        catch (Exception e)
        {
            return string.Empty;
        }
    }


    public async Task<bool> InstallWindowsService(InstallWindowsServiceDto installWindowsServiceDto)
    {
        _connectionId=installWindowsServiceDto.ConnectionId;

        await Log("开始解压文件夹");

        var targetDir = Path.GetDirectoryName(installWindowsServiceDto.ExeFullPath);
        // 查看目录下是否有文件
        if (Directory.Exists(targetDir))
        {
            var files = Directory.GetFiles(targetDir);
            if (files.Length > 0)
                throw new BusinessException("指定目录下已存在文件,请清空后再试");
        }
        else
        {
            Directory.CreateDirectory(targetDir);
        }
        var readStream =  installWindowsServiceDto.File.OpenReadStream();
        using ArchiveFile archiveFile = new ArchiveFile(readStream);
        archiveFile.Extract(targetDir); // extract all
        await Log("解压完成,开始安装");
        
        // 安装服务
        var rt = ServiceInstallerHelper.NssmInstall(installWindowsServiceDto.ServiceName, installWindowsServiceDto.Params
            , installWindowsServiceDto.ExeFullPath, "Auto", installWindowsServiceDto.ServiceDescription, async log=> await Log(log));
        await Log("处理完成");
        return rt;
    }
}