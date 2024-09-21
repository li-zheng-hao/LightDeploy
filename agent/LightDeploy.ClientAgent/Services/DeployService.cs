using System.Security.Cryptography;
using System.ServiceProcess;
using System.Text.Json;
using Flurl.Http;
using LightApi.Infra.Extension;
using LightApi.Infra.InfraException;
using LightDeploy.ClientAgent.Domain;
using LightDeploy.ClientAgent.Dto;
using LightDeploy.Common.Helper;
using Masuit.Tools;
using Polly;
using Serilog;
using SevenZipExtractor;
using SqlSugar;

namespace LightDeploy.ClientAgent.Services;

public class DeployService
{
    private readonly ILogger<DeployService> _logger;
    private readonly ISqlSugarClient _sqlSugarClient;

    public DeployService(ILogger<DeployService> logger, ISqlSugarClient sqlSugarClient)
    {
        _logger = logger;
        _sqlSugarClient = sqlSugarClient;
    }
    public async Task Deploy(DeployDto deployDto)
    {
        var subDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tmp", Guid.NewGuid().ToString("N"));
        var backupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tmp", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(subDir);
        Directory.CreateDirectory(backupDir);
        string exeDir = string.Empty;
        try
        {
            var fileInfoDtos = JsonSerializer.Deserialize<List<FileHelper.FileInfoDto>>(deployDto.FileInfos);
            var readStream = deployDto.File.OpenReadStream();
            using ArchiveFile archiveFile = new ArchiveFile(readStream);
            archiveFile.Extract(subDir); // extract all
            Log.Information("解压完成");
            if (deployDto.TargetDir.IsNullOrWhiteSpace() && deployDto.OnlyCopyFiles != true)
            {
                // 获取当前系统服务
                var services = ServiceController.GetServices();
                var service = services.FirstOrDefault(it => it.ServiceName == deployDto.ServiceName);
                if (service is null)
                {
                    Log.Information("服务不存在");
                    throw new BusinessException("服务不存在");

                }

                Log.Information("正在停止服务...");

                WindowsServiceHelper.StopService(deployDto.ServiceName);
                Log.Information("停止服务完成");

                string exePath = WindowsServiceHelper.GetWindowsServiceLocation(deployDto.ServiceName);
                if (exePath == string.Empty)
                    throw new BusinessException("服务路径不存在");

                _logger.LogInformation($"服务路径:{exePath}");
                exeDir = Path.GetDirectoryName(exePath)!;
            }
            else
            {
                exeDir = deployDto.TargetDir!;
            }


            if (deployDto.SkipBackup != true)
            {
                Log.Information("开始备份文件");

                await Backup(exeDir, backupDir, fileInfoDtos);
            }


            int retryCount = 1;
            Polly.Retry.RetryPolicy retryPolicy =
                Policy.Handle<Exception>().WaitAndRetry(10, i => TimeSpan.FromSeconds(3), async (ex, context) =>
                {
                    Log.Information($"复制文件失败 {ex.Message} 重试次数: {retryCount++}/10");
                });

            retryPolicy.Execute(() => { CopyFilesRecursively(subDir, exeDir); });

            if (deployDto.OnlyCopyFiles != true)
            {
                Log.Information("开始启动服务");

                var errStr = WindowsServiceHelper.StartService(deployDto.ServiceName);

                if (!string.IsNullOrWhiteSpace(errStr)) Log.Error($"启动服务异常：{errStr}");

                Log.Information("启动服务完成");
            }

            if (!string.IsNullOrWhiteSpace(deployDto.HealthCheckUrl))
            {
                Log.Information("开始健康检查");

                // 健康检查
                var result = await HealthCheck(deployDto.HealthCheckUrl);
                if (result == false)
                {
                    if (deployDto.SkipBackup == true)
                    {
                        Log.Information("健康检查未通过,且未备份数据,无法回滚，请手动处理");
                    }
                    else
                    {
                        Log.Information("健康检查未通过,开始回滚");
                        WindowsServiceHelper.StopService(deployDto.ServiceName, 30000);
                        Restore(exeDir, backupDir);
                        WindowsServiceHelper.StartService(deployDto.ServiceName, 30000);
                        Log.Information("健康检查回滚完成,请手动检查服务是否正常");
                        throw new BusinessException("健康检查未通过");
                    }
                   
                }
                Log.Information("健康检查完成");
            }

            Log.Information("记录新文件md5");
            UpdateNewFileRecord(deployDto.ServiceName, fileInfoDtos);
            Log.Information("记录新文件md5完成");
        }
        catch (Exception e)
        {
            Log.Error(e, "发布出现未处理异常" + e.Message + e.StackTrace);
            _logger.LogError(e, e.Message);
            throw;
        }
        finally
        {
            Directory.Delete(subDir, true);
            Directory.Delete(backupDir, true);
        }

    }

    private void UpdateNewFileRecord(string serviceName, List<FileHelper.FileInfoDto>? fileInfoDtos)
    {
        if (fileInfoDtos == null) return;
        var items = _sqlSugarClient.Queryable<FileRecord>().Where(it => it.ServiceName == serviceName).ToList();
        var timestamp = DateTime.UtcNow.ToFileTimeUtc();
        foreach (var fileInfoDto in fileInfoDtos)
        {
            var item = items.FirstOrDefault(it => it.RelativeDirectory == fileInfoDto.RelativeDirectory && it.FileName == fileInfoDto.FileName);
            if (item != null)
            {
                item.MD5 = fileInfoDto.MD5??string.Empty;
                item.PublishTimestamp = timestamp;
                _sqlSugarClient.Updateable(item).ExecuteCommand();
            }
            else
            {
                item = new FileRecord()
                {
                    ServiceName = serviceName,
                    PublishTimestamp = timestamp,
                    AbsoluteDirectory = fileInfoDto.AbsoluteDirectory??string.Empty,
                    RelativeDirectory = fileInfoDto.RelativeDirectory,
                    FileName = fileInfoDto.FileName,
                    MD5 = fileInfoDto.MD5??string.Empty,
                };
                _sqlSugarClient.Insertable(item).ExecuteCommand();
            }
        }
    }

    private async Task<bool> HealthCheck(string deployDtoHealthCheckUrl)
    {
        for (int i = 0; i < 10; i++)
        {
            try
            {
                var response = await deployDtoHealthCheckUrl.AllowAnyHttpStatus().WithTimeout(2).GetAsync();
                if (response.ResponseMessage.IsSuccessStatusCode)
                {
                    Log.Information("健康检查通过,服务已启动");
                    return true;
                }
            }
            catch
            {
                // ignored
            }

            Log.Information($"健康检查失败,休息3秒再检查 第{i + 1}次/10");

            await Task.Delay(3000);
        }

        return false;
    }

    private void Restore(string exeDir, string backupDir)
    {
        if (string.IsNullOrWhiteSpace(exeDir) || string.IsNullOrWhiteSpace(backupDir))
            return;
        if (!Directory.Exists(backupDir))
            return;
        if (!Directory.Exists(exeDir))
            return;
        CopyFilesRecursively(backupDir, exeDir);
    }

    /// <summary>
    /// 备份文件
    /// </summary>
    /// <param name="exeDir"></param>
    /// <param name="backupDir"></param>
    /// <param name="deployDtoFileInfos"></param>
    /// <exception cref="NotImplementedException"></exception>
    private async Task Backup(string exeDir, string backupDir, List<FileHelper.FileInfoDto> deployDtoFileInfos)
    {
        Log.Information($"备份文件夹 {backupDir}");
        foreach (var deployDtoFileInfo in deployDtoFileInfos)
        {
            var filePath = Path.Combine(exeDir, deployDtoFileInfo.RelativeDirectory, deployDtoFileInfo.FileName);
            var targetDir = Path.Combine(backupDir, deployDtoFileInfo.RelativeDirectory);
            if (File.Exists(filePath))
            {
                Directory.CreateDirectory(targetDir);
                Log.Information($"备份文件: {Path.Combine(targetDir, deployDtoFileInfo.FileName)}");
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
                subdir = Directory.CreateDirectory(Path.Combine(targetDir, directoryInfo.Name));
            }
            CopyFilesRecursively(directoryInfo.FullName, subdir.FullName);
        }


        foreach (var sourceFile in sourceDirInfo.GetFiles())
        {
            var exeFile = exeDirInfo.GetFiles().FirstOrDefault(it => it.Name == sourceFile.Name);
            if (exeFile is null)
            {
                File.Copy(sourceFile.FullName, Path.Combine(targetDir, sourceFile.Name));
                Log.Information($"复制文件到{Path.Combine(targetDir, sourceFile.Name)}");
                _logger.LogInformation($"复制文件到{Path.Combine(targetDir, sourceFile.Name)}");
            }
            else
            {

                File.Copy(sourceFile.FullName, Path.Combine(targetDir, sourceFile.Name), true);
                _logger.LogInformation($"复制文件到{Path.Combine(targetDir, sourceFile.Name)}");
                Log.Information($"复制文件到{Path.Combine(targetDir, sourceFile.Name)}");
            }
        }
    }

    public List<FileHelper.FileInfoDto> GetFileInfos(string serviceName, string ignoreFileExtensions)
    {
        string[] ignoreArr = new[] { ".log", ".db", ".db-shm", ".db-wal" };
        if (!string.IsNullOrWhiteSpace(ignoreFileExtensions))
        {
            ignoreArr = ignoreFileExtensions.Split(",");
        }
        string exePath = WindowsServiceHelper.GetWindowsServiceLocation(serviceName);

        if (exePath == string.Empty)
            throw new BusinessException("服务路径不存在");

        string exeDir = Path.GetDirectoryName(exePath)!;
        var exeDirInfo = new DirectoryInfo(exeDir);
        var fileInfos = exeDirInfo.GetFiles("*.*", SearchOption.AllDirectories);
        var fileInfoDtos = fileInfos.Select(it => new FileHelper.FileInfoDto()
        {
            FileName = it.Name,
            RelativeDirectory = Path.GetDirectoryName(it.FullName)!.Replace(exeDir, string.Empty),
            FileSize = it.Length,
            AbsoluteDirectory = Path.GetDirectoryName(it.FullName),
            LastWriteTime = it.LastWriteTime,
            MD5 = GetFileMd5(it.FullName, ignoreArr)
        }).ToList();
        foreach (var fileInfoDto in fileInfoDtos.Where(it => it.RelativeDirectory.StartsWith("/") || it.RelativeDirectory.StartsWith("\\")))
        {
            fileInfoDto.RelativeDirectory = fileInfoDto.RelativeDirectory.Substring(1);
        }
        return fileInfoDtos;
    }

    public List<FileHelper.FileInfoDto> CompareFileInfos(string serviceName, List<FileHelper.FileInfoDto> fileInfoDtos)
    {
        string exePath = WindowsServiceHelper.GetWindowsServiceLocation(serviceName);

        if (exePath == string.Empty)
            throw new BusinessException("服务路径不存在");

        string exeDir = Path.GetDirectoryName(exePath)!;
        List<FileHelper.FileInfoDto> result = new();
        var dbFileRecords = _sqlSugarClient.Queryable<FileRecord>().Where(it => it.ServiceName == serviceName).ToList();
        if (dbFileRecords.IsNullOrEmpty())
        {
            dbFileRecords = ScandAndCalInitMd5Records(serviceName, exeDir);
        }
        foreach (var fileInfoDto in fileInfoDtos)
        {
            var filePath = Path.Combine(exeDir, fileInfoDto.RelativeDirectory, fileInfoDto.FileName);

            var dbFileRecord = dbFileRecords.FirstOrDefault(it =>
                it.RelativeDirectory == fileInfoDto.RelativeDirectory && it.FileName == fileInfoDto.FileName);
            if (dbFileRecord != null)
            {
                if (dbFileRecord.MD5 != fileInfoDto.MD5)
                {
                    result.Add(fileInfoDto);
                }
            }
            else
            {
                if (!File.Exists(filePath))
                {
                    result.Add(fileInfoDto);
                }
                else
                {
                    FileInfo fileInfo = new FileInfo(filePath);
                    if (fileInfo.Length != fileInfoDto.FileSize)
                    {
                        result.Add(fileInfoDto);
                    }
                    else
                    {
                        var md5 = GetFileMd5(filePath, ".log", ".db", ".db-shm", ".db-wal");
                        if (md5 != fileInfoDto.MD5)
                        {
                            result.Add(fileInfoDto);
                        }
                    }
                }
            }

        }
        return result;

    }
    /// <summary>
    /// 第一次扫描文件夹下所有文件的md5，并存储
    /// </summary>
    /// <param name="serviceName"></param>
    /// <param name="exeDir"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private List<FileRecord> ScandAndCalInitMd5Records(string serviceName, string exeDir)
    {
        _logger.LogInformation($"开始扫描文件夹下所有文件的md5");

        List<FileRecord> res = new();
        var fileInfos = FileHelper.GetFileInfos(exeDir);
        foreach (var fileInfoDto in fileInfos)
        {
            var item = new FileRecord()
            {
                FileName = fileInfoDto.FileName,
                AbsoluteDirectory = fileInfoDto.AbsoluteDirectory ?? string.Empty,
                RelativeDirectory = fileInfoDto.RelativeDirectory ?? string.Empty,
                MD5 = fileInfoDto.MD5,
                PublishTimestamp = 0,
                ServiceName = serviceName,
            };
            res.Add(item);
        }
        _logger.LogInformation($"第一次扫描文件夹下所有文件的md5,扫描文件数量 {res.Count}");

        if (res.Any())
        {
            _sqlSugarClient.Insertable<FileRecord>(res).ExecuteCommand();
        }

        return res;
    }

    public List<FileHelper.FileInfoDto> CompareFileInfosInDir(string dir, List<FileHelper.FileInfoDto> fileInfoDtos)
    {
        if (!Directory.Exists(dir))
        {
            throw new BusinessException("目录不存在");
        }

        List<FileHelper.FileInfoDto> result = new();
        foreach (var fileInfoDto in fileInfoDtos)
        {
            var filePath = Path.Combine(dir, fileInfoDto.RelativeDirectory, fileInfoDto.FileName);
            if (!File.Exists(filePath))
            {
                result.Add(fileInfoDto);
            }
            else
            {
                FileInfo fileInfo = new FileInfo(filePath);
                if (fileInfo.Length != fileInfoDto.FileSize)
                {
                    result.Add(fileInfoDto);
                }
                else
                {
                    var md5 = GetFileMd5(filePath, ".log", ".db", ".db-shm", ".db-wal");
                    if (md5 != fileInfoDto.MD5)
                    {
                        result.Add(fileInfoDto);
                    }
                }
            }
        }
        return result;

    }
    /// <summary>
    /// 计算文件MD5
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetFileMd5(string path, params string[] ignoreExtensions)
    {

        if (ignoreExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
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
        Log.Information("开始解压文件夹");

        var targetDir = Path.GetDirectoryName(installWindowsServiceDto.ExeFullPath);
        // 查看目录下是否有文件
        if (Directory.Exists(targetDir))
        {
            var files = Directory.GetFiles(targetDir);

            if (files.Length > 0)
            {
                Log.Error("指定目录下已存在文件,请清空后再试");
                throw new BusinessException("指定目录下已存在文件,请清空后再试");
            }
        }
        else
        {
            Directory.CreateDirectory(targetDir);
        }
        var readStream = installWindowsServiceDto.File.OpenReadStream();
        using ArchiveFile archiveFile = new ArchiveFile(readStream);
        archiveFile.Extract(targetDir); // extract all
        Log.Information("解压完成,开始安装");

        // 安装服务
        var rt = ServiceInstallerHelper.NssmInstall(installWindowsServiceDto.ServiceName, installWindowsServiceDto.Params
            , installWindowsServiceDto.ExeFullPath, "Auto", installWindowsServiceDto.ServiceDescription, async log => Log.Information(log));
        Log.Information("处理完成");
        return rt;
    }


}