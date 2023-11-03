using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using LightDeploy.ClientAgent.Dto;

namespace LightDeployApp;

public class FileHelper
{
    public static List<FileInfoDto> GetFileInfos(string dir)
    {
        var exeDirInfo = new DirectoryInfo(dir);
        var fileInfos = exeDirInfo.GetFiles("*.*", SearchOption.AllDirectories);
        var fileInfoDtos = fileInfos.Where(it=>it.Extension!=".log").Select(it => new FileInfoDto()
        {
            FileName = it.Name,
            RelativeDirectory = Path.GetDirectoryName(it.FullName)!.Replace(dir, string.Empty),
            FileSize = it.Length,
            AbsoluteDirectory = Path.GetDirectoryName(it.FullName),
            LastWriteTime = it.LastWriteTime,
            MD5 = GetFileMd5(it.FullName)
        }).ToList();
        foreach (var fileInfoDto in fileInfoDtos.Where(it=>it.RelativeDirectory.StartsWith("/")||it.RelativeDirectory.StartsWith("\\")))
        {
            fileInfoDto.RelativeDirectory=fileInfoDto.RelativeDirectory.Substring(1);
        }
        return fileInfoDtos;
    }

    public static MemoryStream CompressFiles(List<(string sourcePath,string entryPath)> files)
    {
        var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var file in files)
            {
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
}