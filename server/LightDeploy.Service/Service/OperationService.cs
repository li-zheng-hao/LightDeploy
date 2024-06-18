using System.Text;
using System.Text.RegularExpressions;
using CliWrap;
using Flurl.Http;
using LightApi.Infra;
using LightApi.Infra.Extension;
using LightApi.SqlSugar;
using LightDeploy.Core.Autofac;
using LightDeploy.Core.Helper;
using LightDeploy.Domain;
using LightDeploy.Service.Dto;
using LightDeployApp.Services;
using Masuit.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace LightDeploy.Service;

public class OperationService : IScopedDependency, ISugarTable
{
    private readonly NotifyService _notifyService;
    private readonly IBaseRepository<DeployHistory> _repository;


    public OperationService(NotifyService notifyService,IBaseRepository<DeployHistory> repository)
    {
        _notifyService = notifyService;
        _repository = repository;
    }

    public async Task Deploy(List<DeployTarget> targets,string deployComment)
    {
        var service = targets.First()!.Service!;
        try
        {
            string dir = string.Empty;
            // 编译发布
            if (service.DeployMode == 0)
            {
                var buildResult = await BuildProject(service);
                if (!buildResult.success) return;
                dir = buildResult.dir;
                await _notifyService.NotifyMessageToUser("编译完成");
            }
            else
            {
                dir = service.ProjectPath;
            }

            await DeployFolder(targets, service, dir);

            if (service.DeployMode == 0 && Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
            
            await _repository.Change<DeployHistory>().InsertAsync(new DeployHistory()
            {
                ServiceId = targets.First().ServiceId,
                Description = deployComment,
                PublishTime = DateTime.Now,
            });
        }
        catch (Exception e)
        {
            await _notifyService.NotifyMessageToUser("部署失败");
            await _notifyService.NotifyMessageToUser(e.Message);
            await _notifyService.NotifyMessageToUser(e.StackTrace ?? "");
        }
    }

    private async Task<(bool success, string dir)> BuildProject(DeployService service)
    {
        var tmpDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tmp", Guid.NewGuid().ToString("N"));
        var isSelfContained = service.IsSelfContained ? "--self-contained" : string.Empty;

        var result = await Cli.Wrap("dotnet")
            .WithArguments($" publish {service.ProjectPath} -c Release -o {tmpDir} {isSelfContained} ")
            .WithStandardOutputPipe(PipeTarget.ToDelegate(_notifyService.NotifyMessageToUser,Encoding.GetEncoding("utf-8")))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(_notifyService.NotifyMessageToUser,Encoding.GetEncoding("utf-8")))
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync();
        return (result.ExitCode == 0, tmpDir);
    }

    private async Task DeployFolder(List<DeployTarget> targets, DeployService service, string buildOutputDir)
    {
        var currentFileInfos = FileHelper.GetFileInfos(buildOutputDir);

        foreach (var deployTarget in targets)
        {
            _notifyService.SetHost(deployTarget.Host);
            
            await _notifyService.NotifyMessageToUser($"-------开始处理【{deployTarget.Host}】------------");
            using var scope = App.ServiceProvider.CreateScope();
            
            var agentService = scope.ServiceProvider.GetRequiredService<AgentService>();
            
            await agentService.InitTargetConnection(deployTarget);

            List<FileHelper.FileInfoDto>? needCopiedFiles = currentFileInfos;
            
            // if (service.TargetDir.IsNotNullOrWhiteSpace())
            // {
            //     needCopiedFiles=await agentService.CompareInDir(currentFileInfos, service.TargetDir);
            //
            // }else
            //      needCopiedFiles=await agentService.Compare(currentFileInfos, service.Name);

          
            if (needCopiedFiles.IsNullOrEmpty())
            {
                await _notifyService.NotifyMessageToUser($"{deployTarget.Host}:{deployTarget.Port}无需更新,跳过");
                
                continue;
            }
            else
            {
                if (service.IgnoreRules.IsNotNullOrWhiteSpace())
                {
                    await _notifyService.NotifyMessageToUser($"正则表达式匹配开始忽略文件，匹配规则:{service.IgnoreRules}");

                    var ignoreRules = service.IgnoreRules!.Split(new[] {'|'});

                    // 正则表达式匹配过滤
                    needCopiedFiles = needCopiedFiles!.Where(it =>
                    {
                        return !ignoreRules.Any(rule => Regex.IsMatch(Path.Combine(it.RelativeDirectory,it.FileName), rule));
                    }).ToList();
                }
                
                foreach (var needCopiedFile in needCopiedFiles!)
                {
                    await _notifyService.NotifyMessageToUser($"需要复制文件【{needCopiedFile.FileName}】");
                }
            }

            var memoryStream = CreateZipFile(needCopiedFiles);
            
            await agentService.Upload(memoryStream, deployTarget.Service.Name, needCopiedFiles,service.TargetDir??"",service.OnlyCopyFiles??false);

            
            await _notifyService.NotifyMessageToUser($"-------处理完成【{deployTarget.Host}】------------");
            
            _notifyService.ClearHost();
        }
    }
 
    
    
    /// <summary>
    /// 打包zip文件内存流
    /// </summary>
    /// <param name="calculateNeedDeployFiles"></param>
    /// <returns></returns>
    private static MemoryStream CreateZipFile(List<FileHelper.FileInfoDto> calculateNeedDeployFiles)
    {
        var fileInfos = calculateNeedDeployFiles.Select(it => (Path.Combine(it.AbsoluteDirectory, it.FileName),
            Path.Combine(it.RelativeDirectory, it.FileName)));
        var memoryStream = FileHelper.CompressFiles(fileInfos.ToList());
        return memoryStream;
    }

    public async Task Install(List<DeployTarget> targets,InstallServiceRequest request)
    {
        var service = targets.First()!.Service!;
        try
        {
            string dir = string.Empty;
            // 编译发布
            if (service.DeployMode == 0)
            {
                var buildResult = await BuildProject(service);
                if (!buildResult.success) return;
                dir = buildResult.dir;
                await _notifyService.NotifyMessageToUser("编译完成");
            }
            else
            {
                dir = service.ProjectPath;
            }

            await InstallService(targets, service,dir, request);

            if (service.DeployMode == 0 && Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
          
        }
        catch (Exception e)
        {
            await _notifyService.NotifyMessageToUser("安装失败");
            await _notifyService.NotifyMessageToUser(e.Message);
            await _notifyService.NotifyMessageToUser(e.StackTrace ?? "");
        }
    }

    /// <summary>
    /// 安装服务
    /// </summary>
    /// <param name="targets"></param>
    /// <param name="service"></param>
    /// <param name="dir"></param>
    /// <exception cref="NotImplementedException"></exception>
    private async Task InstallService(List<DeployTarget> targets, DeployService service, string dir, InstallServiceRequest request)
    {
        var currentFileInfos = FileHelper.GetFileInfos(dir);

        foreach (var deployTarget in targets)
        {
            using var scope = App.ServiceProvider.CreateScope();
            
            var agentService = scope.ServiceProvider.GetRequiredService<AgentService>();
            
            await agentService.InitTargetConnection(deployTarget);
            
            
                foreach (var needCopiedFile in currentFileInfos!)
                {
                    await _notifyService.NotifyMessageToUser($"需要复制文件【{needCopiedFile.FileName}】");
                }

            await _notifyService.NotifyMessageToUser($"开始制作安装包");

            var memoryStream = CreateZipFile(currentFileInfos);
            
            await _notifyService.NotifyMessageToUser($"开始上传安装包，开始安装");

            await agentService.Install(memoryStream, deployTarget,service,request);

        }
    }

    public async Task StartService(DeployTarget deployTarget)
    {
        using var scope = App.ServiceProvider.CreateScope();
            
        var agentService = scope.ServiceProvider.GetRequiredService<AgentService>();
            
        await agentService.InitTargetConnection(deployTarget);

        await agentService.StartService(deployTarget.Service.Name);

        await _notifyService.NotifyMessageToUser("操作完毕");
    }

    public async Task StopService(DeployTarget deployTarget)
    {
        using var scope = App.ServiceProvider.CreateScope();
            
        var agentService = scope.ServiceProvider.GetRequiredService<AgentService>();
            
        await agentService.InitTargetConnection(deployTarget);

        await agentService.StopService(deployTarget.Service.Name);
        
        await _notifyService.NotifyMessageToUser("操作完毕");

    }
    /// <summary>
    /// 更新Agent
    /// </summary>
    /// <param name="targets"></param>
    /// <param name="zipFilePath"></param>
    /// <exception cref="NotImplementedException"></exception>
    public async Task UpdateAgent(List<DeployTarget> targets, string? zipFilePath)
    {
        foreach (var deployTarget in targets)
        {
            await _notifyService.NotifyMessageToUser($"更新{deployTarget.Host}开始");

            using var scope = App.ServiceProvider.CreateScope();
            
            var agentService = scope.ServiceProvider.GetRequiredService<AgentService>();
            
            await agentService.InitTargetConnection(deployTarget);
            
            
            await agentService.UpdateAgent(zipFilePath, deployTarget);

            await _notifyService.NotifyMessageToUser($"更新{deployTarget.Host}完成");
        }
    }
}