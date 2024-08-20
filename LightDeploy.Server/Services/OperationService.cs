using System.Text;
using System.Text.RegularExpressions;
using CliWrap;
using LightApi.SqlSugar;
using LightDeploy.Server.Core;
using LightDeploy.Server.Domain;
using LightDeploy.Server.Dtos;
using Masuit.Tools;
using Serilog;

namespace LightDeploy.Server.Services;

public class OperationService
{
    private readonly AgentService _agentService;
    private readonly IBaseRepository<DeployHistory> _repository;
    private readonly NotifyService _notifyService;


    public OperationService(AgentService agentService, IBaseRepository<DeployHistory> repository,
        NotifyService notifyService)
    {
        _agentService = agentService;
        _repository = repository;
        _notifyService = notifyService;
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
                _notifyService.NotifyMessageToUser("编译完成");
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

            await _repository.Change<DeployHistory>().InsertAsync(new DeployHistory()
            {
                ServiceId = targets.First().ServiceId,
                Description = deployComment,
                PublishTime = DateTime.Now,
            });
        }
        catch (TaskCanceledException )
        {
            _notifyService.NotifyMessageToUser("任务已取消");
        }
        catch (OperationCanceledException )
        {
            _notifyService.NotifyMessageToUser("任务已取消");
        }
        catch (Exception e)
        {
            if (e.InnerException is TaskCanceledException)
            {
                _notifyService.NotifyMessageToUser("任务已取消");
            }
            else
            {
                _notifyService.NotifyMessageToUser("部署失败");
                _notifyService.NotifyMessageToUser(e.Message);
                _notifyService.NotifyMessageToUser(e.StackTrace ?? "");
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
            _notifyService.NotifyMessageToUser($"工作目录:{workingDir}");
            File.WriteAllText(scriptFile, serviceBeforeScript);
            var res = await Cli.Wrap("C:\\Windows\\SysWOW64\\WindowsPowerShell\\v1.0\\powershell.exe")
                .WithWorkingDirectory(workingDir)
                .WithArguments(scriptFile)
                .WithStandardOutputPipe(PipeTarget.ToDelegate(s => _notifyService.NotifyMessageToUser(s)))
                .WithStandardErrorPipe(PipeTarget.ToDelegate(s =>
                {
                    _notifyService.NotifyMessageToUser(s);
                }))
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync(token);
            _notifyService.NotifyMessageToUser($"脚本执行返回状态码：{res.ExitCode}");
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
            .WithStandardOutputPipe(PipeTarget.ToDelegate(_notifyService.NotifyMessageToUser,
                Encoding.GetEncoding("utf-8")))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(_notifyService.NotifyMessageToUser,
                Encoding.GetEncoding("utf-8")))
            .WithValidation(CommandResultValidation.ZeroExitCode)
            .ExecuteAsync(deployContext.CancellationTokenSource?.Token??default);
        return (result.ExitCode == 0, tmpDir);
    }

    private async Task DeployFolder(List<DeployTarget> targets, DeployService service, string buildOutputDir,DeployContext deployContext)
    {
        var currentFileInfos = FileHelper.GetFileInfos(buildOutputDir);

        foreach (var deployTarget in targets)
        {
            _notifyService.SetHost(deployTarget.Host);

            _notifyService.NotifyMessageToUser($"-------开始处理【{deployTarget.Host}】------------");

            await _agentService.InitTargetConnection(deployTarget,deployContext.CancellationTokenSource?.Token??default);

            List<FileHelper.FileInfoDto>? needCopiedFiles;

            if (!string.IsNullOrWhiteSpace(service.TargetDir))
            {
                needCopiedFiles = await _agentService.CompareInDir(currentFileInfos, service.TargetDir!,deployContext.CancellationTokenSource?.Token??default);
            }
            else
                needCopiedFiles = await _agentService.Compare(currentFileInfos, service.Name,deployContext.CancellationTokenSource?.Token??default);


            if (needCopiedFiles.IsNullOrEmpty())
            {
                _notifyService.NotifyMessageToUser($"{deployTarget.Host}:{deployTarget.Port}无需更新,跳过");

                continue;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(service.IgnoreRules))
                {
                    _notifyService.NotifyMessageToUser($"正则表达式匹配开始忽略文件，匹配规则:{service.IgnoreRules}");

                    var ignoreRules = service.IgnoreRules!.Split(new[] { '|' });

                    // 正则表达式匹配过滤
                    needCopiedFiles = needCopiedFiles!.Where(it =>
                    {
                        return !ignoreRules.Any(rule =>
                            Regex.IsMatch(Path.Combine(it.RelativeDirectory, it.FileName), rule));
                    }).ToList();
                }

                foreach (var needCopiedFile in needCopiedFiles!)
                {
                    _notifyService.NotifyMessageToUser($"需要复制文件【{needCopiedFile.FileName}】");
                }
            }

            var memoryStream = FileHelper.CreateZipFile(needCopiedFiles,deployContext.CancellationTokenSource?.Token??default);

            deployContext.CanCanel = false;
            
            _notifyService.NotifyMessageToUser($"开始上传并部署文件");

            await _agentService.Upload(memoryStream, service.Name, needCopiedFiles,
                service.TargetDir ?? "", service.OnlyCopyFiles ?? false,service.EnableHealthCheck);

            _notifyService.NotifyMessageToUser($"-------处理完成【{deployTarget.Host}】------------");

            _notifyService.ClearHost();

            await _agentService.DisConnect();
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
                _notifyService.NotifyMessageToUser("编译完成");
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
            _notifyService.NotifyMessageToUser("操作已取消");
        }
        catch (Exception e)
        {
            _notifyService.NotifyMessageToUser("安装失败");
            _notifyService.NotifyMessageToUser(e.Message);
            _notifyService.NotifyMessageToUser(e.StackTrace ?? "");
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
            await _agentService.InitTargetConnection(deployTarget,deployContext.CancellationTokenSource?.Token??default);


            foreach (var needCopiedFile in currentFileInfos!)
            {
                _notifyService.NotifyMessageToUser($"需要复制文件【{needCopiedFile.FileName}】");
            }

            _notifyService.NotifyMessageToUser($"开始制作安装包");

            var memoryStream =FileHelper.CreateZipFile(currentFileInfos,deployContext.CancellationTokenSource?.Token??default);

            _notifyService.NotifyMessageToUser($"开始上传安装包，开始安装");

            await _agentService.Install(memoryStream, deployTarget, service, request);

            _notifyService.NotifyMessageToUser($"安装完成");
        }
    }

    public async Task StartService(DeployTarget deployTarget,string serviceName)
    {
        await _agentService.InitTargetConnection(deployTarget,default);

        await _agentService.StartService(serviceName);

        _notifyService.NotifyMessageToUser("操作完毕");

        await _agentService.DisConnect();
    }

    public async Task StopService(DeployTarget deployTarget,string serviceName)
    {
        await _agentService.InitTargetConnection(deployTarget,default);

        await _agentService.StopService(serviceName);

        _notifyService.NotifyMessageToUser("操作完毕");
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
             _notifyService.NotifyMessageToUser($"更新{deployTarget.Host}开始");

            await  _agentService.InitTargetConnection(deployTarget,default);

            await  _agentService.UpdateAgent(zipFilePath, deployTarget);

             _notifyService.NotifyMessageToUser($"更新{deployTarget.Host}完成");

             await _agentService.DisConnect();
        }
    }
    
    /// <summary>
    /// 更新Agent
    /// </summary>
    public async Task UpdateAgent(DeployTarget target, string? zipFilePath)
    {
        _notifyService.NotifyMessageToUser($"更新{target.Host}开始");

        try
        {
            await  _agentService.InitTargetConnection(target,default);

            await  _agentService.UpdateAgent(zipFilePath, target);

            await _agentService.DisConnect();
        }
        catch (Exception e)
        {
            Log.Error(e,e.Message);
            _notifyService.NotifyMessageToUser($"更新代理失败:{e.Message}:{e.StackTrace}");
            return ;
        }
        _notifyService.NotifyMessageToUser($"更新{target.Host}完成");

    }

    public async Task<string> GetStatus(DeployTarget deployTarget,string serviceName)
    {
        
        await _agentService.InitTargetConnection(deployTarget,default);
        try
        {
            string? status=await _agentService.GetStatus(serviceName);

            await _agentService.DisConnect();
            _notifyService.NotifyMessageToUser($"查询到目标：{deployTarget.Host}服务器上的服务状态为{status}");
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
            string? status=await _agentService.GetAgentVersion(deployTarget);

            return status??"查询失败";
        }
        catch (Exception e)
        {
            Log.Error(e,e.Message);
            _notifyService.NotifyMessageToUser($"{deployTarget.Host}获取版本失败:{e.Message}");
            return "查询失败";
        }
        
    }
}