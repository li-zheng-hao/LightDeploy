using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Masuit.Tools;
using Serilog;
using SevenZipExtractor;

namespace LightDeploy.Common.Helper;

public static class FileHelper
{
    /// <summary>
    /// 打包zip文件内存流
    /// </summary>
    /// <param name="calculateNeedDeployFiles"></param>
    /// <returns></returns>
    public static MemoryStream CreateZipFile(List<FileHelper.FileInfoDto> calculateNeedDeployFiles, CancellationToken cancellationToken = default)
    {
        var fileInfos = calculateNeedDeployFiles.Select(it => (Path.Combine(it.AbsoluteDirectory!, it.FileName),
            Path.Combine(it.RelativeDirectory, it.FileName)));
        var memoryStream = FileHelper.CompressFiles(fileInfos.ToList(), cancellationToken);
        return memoryStream;
    }
    public static string GetMD5(Stream stream)
    {
        using var md5 = MD5.Create();
        stream.Position = 0;
        var hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// 删除 不存在则忽略
    /// </summary>
    /// <param name="path"></param>
    public static void Delete(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }
    /// <summary>
    /// 递归删除 不存在则忽略
    /// </summary>
    /// <param name="path"></param>
    public static void DeleteDirectory(string path, bool recursive = true)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive);
    }

    public static List<(string fileName, Stream stream)> DecompressFilesBy7Zip(Stream inputStream,
        params string[] exts)
    {
        List<(string fileName, Stream stream)> result = new();
        using var archiveFile = new ArchiveFile(inputStream);
        foreach (var entry in archiveFile.Entries)
        {
            if (!exts.Contains(Path.GetExtension(entry.FileName), StringComparer.OrdinalIgnoreCase))
                continue;
            var memoryStream = new MemoryStream();
            entry.Extract(memoryStream);
            result.Add((entry.FileName, memoryStream));
        }
        return result;
    }

    public static Stream CompressFiles(List<(Stream Stream, string fileName)> files)
    {
        var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var file in files)
            {
                var zipArchiveEntry = archive.CreateEntry(file.fileName);
                using var zipStream = zipArchiveEntry.Open();
                file.Stream.CopyTo(zipStream);
            }
        }

        memoryStream.Position = 0;
        return memoryStream;
    }

    public static List<FileInfoDto> GetFileInfos(string dir, string? serviceIgnoreRules=null)
    {
        var exeDirInfo = new DirectoryInfo(dir);
        var fileInfos = exeDirInfo.GetFiles("*.*", SearchOption.AllDirectories);
        Log.Information($"获取文件信息,文件总数：{fileInfos.Count()}");
        var fileInfoDtos = fileInfos
            .Where(it=>new[]{".log", ".db", ".db-shm", ".db-wal"}.All(ext=>!ext.Equals(it.Extension, StringComparison.OrdinalIgnoreCase)))
            .WhereIf(serviceIgnoreRules.IsNullOrEmpty()==false,it =>
            {
                var ignoreRules = serviceIgnoreRules!.Split(new[] { '|' });
                return ignoreRules.All(rule=>!Regex.IsMatch(it.FullName, rule));
            })
            .Select(it =>
            {
                return new FileInfoDto()
                {
                    FileName = it.Name,
                    RelativeDirectory = Path.GetDirectoryName(it.FullName)!.Replace(dir, string.Empty),
                    FileSize = it.Length,
                    AbsoluteDirectory = Path.GetDirectoryName(it.FullName),
                    LastWriteTime = it.LastWriteTime,
                    MD5 = GetFileMd5(it.FullName)
                };
            }).ToList();
        foreach (var fileInfoDto in fileInfoDtos.Where(it => it.RelativeDirectory.StartsWith("/") || it.RelativeDirectory.StartsWith("\\")))
        {
            fileInfoDto.RelativeDirectory = fileInfoDto.RelativeDirectory.Substring(1);
        }
        return fileInfoDtos;
    }

    public static MemoryStream CompressFiles(List<(string sourcePath, string entryPath)> files,
        CancellationToken cancellationToken = default)
    {
        var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var file in files)
            {
                if (cancellationToken.IsCancellationRequested) throw new TaskCanceledException();

                var zipArchiveEntry = archive.CreateEntry(file.entryPath);
                using var zipStream = zipArchiveEntry.Open();

                using var stream = File.OpenRead(file.sourcePath);
                stream.CopyTo(zipStream);
            }
        }

        memoryStream.Position = 0;
        return memoryStream;
    }

    /// <summary>
    /// 计算文件MD5
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetFileMd5(string path)
    {
        using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var md5 = MD5.Create();
        var hash = md5.ComputeHash(fileStream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
    public class FileInfoDto
    {
        public string? AbsoluteDirectory { get; set; }
        /// <summary>
        /// 相对指定路径的相对目录
        /// </summary>
        public string RelativeDirectory { get; set; }
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime LastWriteTime { get; set; }

        public string MD5 { get; set; }
    }
}