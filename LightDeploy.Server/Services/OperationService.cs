using System.Text;
using System.Text.RegularExpressions;
using CliWrap;
using LightApi.EFCore.Repository;
using LightApi.Infra;
using LightDeploy.Common.Helper;
using LightDeploy.Server.Core;
using LightDeploy.Server.Domain;
using LightDeploy.Server.Dtos;
using Masuit.Tools;
using Serilog;

namespace LightDeploy.Server.Services;

public class OperationService
{
    private readonly IEfRepository<DeployHistory> _repository;
    public NotifyService NotifyService;


    public OperationService(IEfRepository<DeployHistory> repository
        )
    {
        _repository = repository;
    }

    
    public async Task Deploy(List<DeployTarget> targets,DeployService deployService, string? deployComment,DeployContext deployContext)
    {
        var service = deployService;
        try
        {
            string dir = string.Empty;
            if (!string.IsNullOrWhiteSpace(service.BeforeScript))
            {
                await RunPsScript(service.BeforeScript,AppDomain.CurrentDomain.BaseDirectory,deployContext.CancellationTokenSource?.Token??default);
            }
            // 编译发布
            if (service.DeployMode == 0)
            {
                var buildResult = await BuildProject(service, deployContext);
                if (!buildResult.success) return;
                dir = buildResult.dir;
                NotifyService.NotifyMessageToUser("编译完成");
            }
            else
            {
                dir = service.ProjectPath;
            }

            await DeployFolder(targets, service, dir,deployContext);

            if (service.DeployMode == 0 && Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }

             _repository.Add(new DeployHistory()
            {
                ServiceId = targets.First().ServiceId,
                Description = deployComment,
                PublishTime = DateTime.Now,
            });
            await _repository.SaveChangesAsync();
        }
        catch (TaskCanceledException )
        {
            NotifyService.NotifyMessageToUser("任务已取消");
        }
        catch (OperationCanceledException )
        {
            NotifyService.NotifyMessageToUser("任务已取消");
        }
        catch (Exception e)
        {
            if (e.InnerException is TaskCanceledException)
            {
                NotifyService.NotifyMessageToUser("任务已取消");
            }
            else
            {
                NotifyService.NotifyMessageToUser("部署失败");
                NotifyService.NotifyMessageToUser(e.Message);
                NotifyService.NotifyMessageToUser(e.StackTrace ?? "");
            }
        }
    }

    private async Task RunPsScript(string serviceBeforeScript,string workingDir, CancellationToken token)
    {
        var tmpDir=Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tmp");
        if (!Directory.Exists(tmpDir))
            Directory.CreateDirectory(tmpDir);
        var scriptFile = Path.Combine(tmpDir, $"{Guid.NewGuid().ToString()}.ps1");
        try
        {
            NotifyService.NotifyMessageToUser($"工作目录:{workingDir}");
            File.WriteAllText(scriptFile, serviceBeforeScript);
            var res = await Cli.Wrap("C:\\Windows\\SysWOW64\\WindowsPowerShell\\v1.0\\powershell.exe")
                .WithWorkingDirectory(workingDir)
                .WithArguments(scriptFile)
                .WithStandardOutputPipe(PipeTarget.ToDelegate(s => NotifyService.NotifyMessageToUser(s)))
                .WithStandardErrorPipe(PipeTarget.ToDelegate(s =>
                {
                    NotifyService.NotifyMessageToUser(s);
                }))
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync(token);
            NotifyService.NotifyMessageToUser($"脚本执行返回状态码：{res.ExitCode}");
            if (!res.IsSuccess ||res.ExitCode!=0)
                throw new Exception("脚本执行失败");
        }
        finally
        {
            if(File.Exists(scriptFile))
                File.Delete(scriptFile);
        }
        
    }

    private async Task<(bool success, string dir)> BuildProject(DeployService service,DeployContext deployContext)
    {
        var tmpDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tmp", Guid.NewGuid().ToString("N"));
        var isSelfContained = service.IsSelfContained ? "--self-contained" : string.Empty;

        var result = await Cli.Wrap("dotnet")
            .WithArguments($" publish {service.ProjectPath} -c Release -o {tmpDir} {isSelfContained} ")
            .WithStandardOutputPipe(PipeTarget.ToDelegate(NotifyService.NotifyMessageToUser,
                Encoding.GetEncoding("utf-8")))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(NotifyService.NotifyMessageToUser,
                Encoding.GetEncoding("utf-8")))
            .WithValidation(CommandResultValidation.ZeroExitCode)
            .ExecuteAsync(deployContext.CancellationTokenSource?.Token??default);
        return (result.ExitCode == 0, tmpDir);
    }

    private async Task DeployFolder(List<DeployTarget> targets, DeployService service, string buildOutputDir,DeployContext deployContext)
    {
        NotifyService.NotifyMessageToUser($"-------开始计算发布文件信息------------");

        var currentFileInfos = FileHelper.GetFileInfos(buildOutputDir,service.IgnoreRules);

        NotifyService.NotifyMessageToUser($"-------计算发布文件信息完成，一共有{currentFileInfos.Count}待比较------------");

        foreach (var deployTarget in targets)
        {
            var agentService = App.GetRequiredService<AgentService>();
            agentService.NotifyService = NotifyService;
            
            NotifyService.SetHost(deployTarget.Host);

            NotifyService.NotifyMessageToUser($"-------开始处理【{deployTarget.Host}】------------");

            await agentService.InitTargetConnection(deployTarget,deployContext.CancellationTokenSource?.Token??default);

            List<FileHelper.FileInfoDto>? needCopiedFiles;
            NotifyService.NotifyMessageToUser("开始对比文件");

            if (!string.IsNullOrWhiteSpace(service.TargetDir))
            {
                needCopiedFiles = await agentService.CompareInDir(currentFileInfos, service.TargetDir!,deployContext.CancellationTokenSource?.Token??default);
            }
            else
                needCopiedFiles = await agentService.Compare(currentFileInfos, service.Name,deployContext.CancellationTokenSource?.Token??default);

            NotifyService.NotifyMessageToUser("文件对比完成");

            if (needCopiedFiles.IsNullOrEmpty())
            {
                NotifyService.NotifyMessageToUser($"{deployTarget.Host}:{deployTarget.Port}无需更新,跳过");

                continue;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(service.IgnoreRules))
                {
                    NotifyService.NotifyMessageToUser($"正则表达式匹配开始忽略文件，匹配规则:{service.IgnoreRules}");

                    var ignoreRules = service.IgnoreRules!.Split(new[] { '|' });

                    // 正则表达式匹配过滤
                    needCopiedFiles = needCopiedFiles!.Where(it =>
                    {
                        return !ignoreRules.Any(rule =>
                            Regex.IsMatch(Path.Combine(it.RelativeDirectory, it.FileName), rule));
                    }).ToList();
                }
                NotifyService.NotifyMessageToUser($"-------文件对比完成，一共有{needCopiedFiles!.Count}个文件待发布------------");

                foreach (var needCopiedFile in needCopiedFiles!)
                {
                    NotifyService.NotifyMessageToUser($"需要复制文件【{needCopiedFile.FileName}】");
                }
            }

            var memoryStream = FileHelper.CreateZipFile(needCopiedFiles,deployContext.CancellationTokenSource?.Token??default);

            deployContext.CanCanel = false;


            bool skipBackup = service.EnvironmentName != "生产";
            NotifyService.NotifyMessageToUser($"开始上传并部署文件,跳过备份:{skipBackup}");
            await agentService.Upload(memoryStream, service.Name, needCopiedFiles,
                service.TargetDir ?? "", service.OnlyCopyFiles ?? false,service.EnableHealthCheck,skipBackup);

            NotifyService.NotifyMessageToUser($"-------处理完成【{deployTarget.Host}】------------");

            NotifyService.ClearHost();

            await agentService.DisConnect();
        }
    }


    

    public async Task Install(List<DeployTarget> targets,DeployService service, InstallServiceRequest request,DeployContext deployContext)
    {
        try
        {
            string dir = string.Empty;
            // 编译发布
            if (service.DeployMode == 0)
            {
                var buildResult = await BuildProject(service, deployContext);
                if (!buildResult.success) return;
                dir = buildResult.dir;
                NotifyService.NotifyMessageToUser("编译完成");
            }
            else
            {
                dir = service.ProjectPath;
            }

            await InstallService(targets, service, dir, request, deployContext);

            if (service.DeployMode == 0 && Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
        }
        catch (OperationCanceledException)
        {
            NotifyService.NotifyMessageToUser("操作已取消");
        }
        catch (Exception e)
        {
            NotifyService.NotifyMessageToUser("安装失败");
            NotifyService.NotifyMessageToUser(e.Message);
            NotifyService.NotifyMessageToUser(e.StackTrace ?? "");
        }
    }

    /// <summary>
    /// 安装服务
    /// </summary>
    /// <param name="targets"></param>
    /// <param name="service"></param>
    /// <param name="dir"></param>
    /// <exception cref="NotImplementedException"></exception>
    private async Task InstallService(List<DeployTarget> targets, DeployService service, string dir,
        InstallServiceRequest request,DeployContext deployContext)
    {
        var currentFileInfos = FileHelper.GetFileInfos(dir);

        foreach (var deployTarget in targets)
        {
            var agentService = App.GetRequiredService<AgentService>();
            
            agentService.NotifyService = NotifyService;
            await agentService.InitTargetConnection(deployTarget,deployContext.CancellationTokenSource?.Token??default);


            foreach (var needCopiedFile in currentFileInfos!)
            {
                NotifyService.NotifyMessageToUser($"需要复制文件【{needCopiedFile.FileName}】");
            }

            NotifyService.NotifyMessageToUser($"开始制作安装包");

            var memoryStream =FileHelper.CreateZipFile(currentFileInfos,deployContext.CancellationTokenSource?.Token??default);

            NotifyService.NotifyMessageToUser($"开始上传安装包，开始安装");

            await agentService.Install(memoryStream, deployTarget, service, request);

            NotifyService.NotifyMessageToUser($"安装完成");
        }
    }

    public async Task StartService(DeployTarget deployTarget,string serviceName)
    {
        var agentService = App.GetRequiredService<AgentService>();
        agentService.NotifyService = NotifyService;
        await agentService.InitTargetConnection(deployTarget,default);

        await agentService.StartService(serviceName);

        NotifyService.NotifyMessageToUser("操作完毕");

        await agentService.DisConnect();
    }

    public async Task StopService(DeployTarget deployTarget,string serviceName)
    {
        var agentService = App.GetRequiredService<AgentService>();
        agentService.NotifyService = NotifyService;
        await agentService.InitTargetConnection(deployTarget,default);

        await agentService.StopService(serviceName);

        NotifyService.NotifyMessageToUser("操作完毕");
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
            var agentService = App.GetRequiredService<AgentService>();
            agentService.NotifyService = NotifyService;
             NotifyService.NotifyMessageToUser($"更新{deployTarget.Host}开始");

            await  agentService.InitTargetConnection(deployTarget,default);

            await  agentService.UpdateAgent(zipFilePath, deployTarget);

             NotifyService.NotifyMessageToUser($"更新{deployTarget.Host}完成");

             await agentService.DisConnect();
        }
    }
    
    /// <summary>
    /// 更新Agent
    /// </summary>
    public async Task UpdateAgent(DeployTarget target, string? zipFilePath)
    {
        NotifyService.NotifyMessageToUser($"更新{target.Host}开始");

        try
        {
            var agentService = App.GetRequiredService<AgentService>();
            agentService.NotifyService = NotifyService;
            await  agentService.InitTargetConnection(target,default);

            await  agentService.UpdateAgent(zipFilePath, target);

            await agentService.DisConnect();
        }
        catch (Exception e)
        {
            Log.Error(e,e.Message);
            NotifyService.NotifyMessageToUser($"更新代理失败:{e.Message}:{e.StackTrace}");
            return ;
        }
        NotifyService.NotifyMessageToUser($"更新{target.Host}完成");

    }
    /// <summary>
    /// 复制文件
    /// </summary>
    public async Task CopyFile(DeployTarget target, string zipFilePath,string targetDir)
    {
        NotifyService.NotifyMessageToUser($"更新{target.Host}开始");

        try
        {
            var agentService = App.GetRequiredService<AgentService>();
            agentService.NotifyService = NotifyService;
            await  agentService.InitTargetConnection(target,default);

            await  agentService.CopyFile(zipFilePath,targetDir, target);

            await agentService.DisConnect();
        }
        catch (Exception e)
        {
            Log.Error(e,e.Message);
            NotifyService.NotifyMessageToUser($"复制文件失败:{e.Message}:{e.StackTrace}");
            return ;
        }
        NotifyService.NotifyMessageToUser($"{target.Host}复制完成");

    }

    public async Task<string> GetStatus(DeployTarget deployTarget,string serviceName)
    {
        
        var agentService = App.GetRequiredService<AgentService>();
        try
        {
            agentService.NotifyService = NotifyService;
            await agentService.InitTargetConnection(deployTarget,default);
       
            string? status=await agentService.GetStatus(serviceName);

            await agentService.DisConnect();
            
            NotifyService.NotifyMessageToUser($"查询到目标：{deployTarget.Host}服务器上的服务状态为{status}");
            return status??"查询失败";
        }
        catch (Exception e)
        {
            Log.Error(e,e.Message);
            return e.Message;
        }
        
    }
    public async Task<string> GetAgentVersion(DeployTarget deployTarget)
    {
        try
        {
            var agentService = App.GetRequiredService<AgentService>();
            agentService.NotifyService = NotifyService;
            string? status=await agentService.GetAgentVersion(deployTarget);

            return status??"查询失败";
        }
        catch (Exception e)
        {
            Log.Error(e,e.Message);
            NotifyService.NotifyMessageToUser($"{deployTarget.Host}获取版本失败:{e.Message}");
            return "查询失败";
        }
        
    }
}