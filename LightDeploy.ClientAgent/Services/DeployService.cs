﻿using System.Management;
using System.ServiceProcess;
using LightDeploy.ClientAgent.Dto;
using Polly;
using SevenZipExtractor;

namespace LightDeploy.ClientAgent.Services;

public class DeployService
{
    private readonly ILogger<DeployService> _logger;

    public DeployService(ILogger<DeployService> logger)
    {
        _logger = logger;
    }
    public async Task Deploy(DeployDto deployDto)
    {
        var subDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tmp", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(subDir);
        try
        {
            var readStream = deployDto.File.OpenReadStream();
            using ArchiveFile archiveFile = new ArchiveFile(readStream);
            archiveFile.Extract(subDir); // extract all
            // 获取当前系统服务
            var services = ServiceController.GetServices();
            var service = services.FirstOrDefault(it => it.ServiceName == deployDto.ServiceName);
            if (service is null)
                throw new BusinessException("服务不存在");
            if(service.Status==ServiceControllerStatus.Running)
                service.Stop(true);
            service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));

            string exePath=WindowServiceHelper.GetWindowsServiceLocation(deployDto.ServiceName);
            if (exePath == string.Empty)
                throw new BusinessException("服务路径不存在");

            _logger.LogInformation($"服务路径:{exePath}");
            string exeDir = Path.GetDirectoryName(exePath)!;
           
            Polly.Retry.RetryPolicy retryPolicy = Policy.Handle<Exception>().WaitAndRetry(3, i => TimeSpan.FromSeconds(2));
            retryPolicy.Execute(() =>
            {
                CopyFilesRecursively(subDir, exeDir);
            });
           
            service.Start();

        }
        catch (Exception e)
        {
            _logger.LogError(e,e.Message);
            throw;
        }
        finally
        {
            Directory.Delete(subDir,true);
        }
        
    }
    /// <summary>
    /// 比较两个目录以及子目录下所有文件,对比文件大小和修改时间，如果不一致则复制
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
                _logger.LogInformation($"复制文件到{Path.Combine(targetDir, sourceFile.Name)}");
            }
            else
            {
                if (sourceFile.Length != exeFile.Length || sourceFile.LastWriteTime != exeFile.LastWriteTime)
                {
                    File.Copy(sourceFile.FullName, Path.Combine(targetDir, sourceFile.Name), true);
                    _logger.LogInformation($"复制文件到{Path.Combine(targetDir, sourceFile.Name)}");
                }
            }
        }
    }

    public List<FileInfoDto> GetFileInfos(string serviceName)
    {
        string exePath=WindowServiceHelper.GetWindowsServiceLocation(serviceName);

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
            LastWriteTime = it.LastWriteTime
        }).ToList();
        foreach (var fileInfoDto in fileInfoDtos.Where(it=>it.RelativeDirectory.StartsWith("/")||it.RelativeDirectory.StartsWith("\\")))
        {
            fileInfoDto.RelativeDirectory=fileInfoDto.RelativeDirectory.Substring(1);
        }
        return fileInfoDtos;
    }
}