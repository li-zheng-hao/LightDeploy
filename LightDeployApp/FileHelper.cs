using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using LightDeploy.ClientAgent.Dto;

namespace LightDeployApp;

public class FileHelper
{
    public static List<FileInfoDto> GetFileInfos(string dir)
    {
        var exeDirInfo = new DirectoryInfo(dir);
        var fileInfos = exeDirInfo.GetFiles("*.*", SearchOption.AllDirectories);
        var fileInfoDtos = fileInfos.Select(it => new FileInfoDto()
        {
            FileName = it.Name,
            RelativeDirectory = Path.GetDirectoryName(it.FullName)!.Replace(dir, string.Empty),
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
}