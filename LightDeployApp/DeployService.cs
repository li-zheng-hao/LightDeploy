﻿using System;
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
        var environments = DBHelper.GetClient().Queryable<TEnvironment>().Where(it => it.Name == deployParams.Environment).ToList();
        foreach (var environment in environments)
        {
            textBox.Text+=$"开始部署{environment.Host}:{environment.Port}\n";
            
            List<FileInfoDto> remoteFiles =await $"http://{environment.Host}:{environment.Port}/api/deploy/listfileinfo"
                .SetQueryParam("serviceName", deployParams.ServiceName)
                .GetJsonAsync<List<FileInfoDto>>();

            var currentFileInfos=FileHelper.GetFileInfos(deployParams.TargetPath);

            var calculateNeedDeployFiles = CalculateNeedDeployFiles(currentFileInfos, remoteFiles);
            if(calculateNeedDeployFiles.Count==0)
            {
                textBox.Text+=$"无需部署{environment.Host}:{environment.Port}\n";
                continue;
            }

            var memoryStream =
                await Task.Run(() => CreateZipFile(calculateNeedDeployFiles)); 
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
            catch (Exception e)
            {
                textBox.Text+=$"部署失败{environment.Host}:{environment.Port}\n";
                continue;
            }
         
            textBox.Text+=$"部署完成{environment.Host}:{environment.Port}\n";
        }
    }

    private static MemoryStream CreateZipFile(List<FileInfoDto> calculateNeedDeployFiles)
    {
        var zipPath = AppDomain.CurrentDomain.BaseDirectory;
        var fileInfos=calculateNeedDeployFiles.Select(it => (Path.Combine(it.AbsoluteDirectory, it.FileName),Path.Combine(it.RelativeDirectory, it.FileName)));
        var memoryStream=FileHelper.CompressFiles(fileInfos.ToList());
        memoryStream.Position = 0;
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