using Flurl.Http;
using Flurl.Http.Configuration;
using LightDeploy.Common.Helper;
using LightDeploy.Server.Core;
using LightDeploy.Server.Domain;
using LightDeploy.Server.Dtos;
using Masuit.Tools;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;

namespace LightDeploy.Server.Services;

public class AgentService : IAsyncDisposable
{
    public NotifyService? NotifyService;
    private HubConnection? connection;

    private DeployTarget _target;


    public async Task InitTargetConnection(DeployTarget target,CancellationToken cancellationToken)
    {
        if (connection != null)
        {
            await DisConnect();
        }
        _target = target;
        connection = new HubConnectionBuilder()
            .WithUrl($"http://{target.Host}:{target.Port}/agent")
            .WithServerTimeout(TimeSpan.FromSeconds(3))
            .WithAutomaticReconnect()
            .Build();
        connection.On("Log", (string msg) => { NotifyService.NotifyMessageToUser($"Agent: {msg}"); });
        var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        tokenSource.CancelAfter(TimeSpan.FromSeconds(2));
        
        await connection.StartAsync(cancellationToken:tokenSource.Token);
    }
    public async Task DisConnect()
    {
        if (connection != null)
        {
            await connection!.DisposeAsync();
            connection = null;
        }
    }

    /// <summary>
    /// 获取HTTP连接
    /// </summary>
    /// <param name="endpoint"></param>
    /// <returns></returns>
    private IFlurlRequest GetHttpClient(string endpoint)
    {
        var client = $"http://{_target.Host}:{_target.Port}/{endpoint}"
                .WithHeader("Authorization", _target.AuthKey ?? "")
                .WithHeader("WsConnectionId", connection?.ConnectionId??string.Empty)
            ;
        client.Settings.JsonSerializer = new NewtonsoftJsonSerializer(new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new DefaultContractResolver(),
            DateFormatString = "yyyy-MM-ddTHH:mm:ssK"
        });
        return client;
    }

    /// <summary>
    /// 对比文件
    /// </summary>
    /// <param name="source"></param>
    /// <param name="serviceName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<List<FileHelper.FileInfoDto>?> Compare(List<FileHelper.FileInfoDto> source, string serviceName,
        CancellationToken cancellationToken )
    {
        var result = await GetHttpClient("api/deploy/compare")
            .SetQueryParam("serviceName", serviceName)
            .WithHeader("Authorization", _target.AuthKey ?? "")
            .PostJsonAsync(source, cancellationToken)
            .ReceiveJson<UnifyResult<List<FileHelper.FileInfoDto>>>();
        if (result.success)
        {
            return result.data;
        }
        else
        {
            NotifyService.NotifyMessageToUser(result.msg);
            return null;
        }
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="memoryStream"></param>
    /// <param name="serviceName"></param>
    /// <param name="calculateNeedDeployFiles"></param>
    /// <param name="skipBackup"></param>
    /// <returns></returns>
    public async Task<bool> Upload(Stream memoryStream, string serviceName,
        List<FileHelper.FileInfoDto> calculateNeedDeployFiles,string targetDir,bool onlyCopyFiles,bool enableHealthCheck,bool skipBackup)
    {
        var response = await GetHttpClient("api/deploy/deploy")
            .WithTimeout(TimeSpan.FromMinutes(5))
            .PostMultipartAsync(mp =>
            {
                mp.AddFile("File", new MemoryStream(memoryStream.ToArray()), "file.zip");
                mp.AddString("ServiceName", serviceName);
                mp.AddJson("FileInfos", calculateNeedDeployFiles);
                mp.AddString("TargetDir", targetDir);
                mp.AddString("OnlyCopyFiles", onlyCopyFiles.ToString());
                mp.AddString("ConnectionId", connection?.ConnectionId ?? "");
                mp.AddString("HealthCheckUrl",
                    enableHealthCheck && !string.IsNullOrWhiteSpace(_target.HealthCheckUrl)
                        ? _target.HealthCheckUrl
                        : "");
                mp.AddString("SkipBackup", skipBackup.ToString());
            })
            .ReceiveJson<UnifyResult<object>>();
        if (!response.success)
        {
            NotifyService.NotifyMessageToUser($"部署失败{_target.Host}:{_target.Port} {response.msg}");
            return false;
        }
        else
            return true;
    }

    public async Task StartService(string serviceName)
    {
        var response = await GetHttpClient("api/deploy/startservice")
            .SetQueryParam("serviceName", serviceName)
            .PostAsync()
            .ReceiveJson<UnifyResult<object>>();
        if (!response.success)
        {
            NotifyService.NotifyMessageToUser(response.msg ?? "启动失败");
        }
    }

    public async Task StopService(string serviceName)
    {
        var response = await GetHttpClient("api/deploy/stopservice")
            .SetQueryParam("serviceName", serviceName)
            .PostAsync()
            .ReceiveJson<UnifyResult<object>>();
        if (!response.success)
        {
            NotifyService.NotifyMessageToUser(response.msg ?? "停止失败");
        }
    }

    public async Task<string?> RefreshService(string serviceName)
    {
        var response = await GetHttpClient("api/deploy/getstatus")
            .WithTimeout(5)
            .SetQueryParam("serviceName", serviceName)
            .GetJsonAsync<UnifyResult<string>>();
        if (response.success)
        {
            return response.data ?? string.Empty;
        }
        else
        {
            NotifyService.NotifyMessageToUser(response.msg ?? "获取状态失败");
            return default;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (connection != null) await connection.DisposeAsync();
    }


    public async Task Install(MemoryStream memoryStream, DeployTarget deployTarget, DeployService service,
        InstallServiceRequest request)
    {
        var resp = await GetHttpClient("api/deploy/installwindowsservice")
            .WithTimeout(TimeSpan.FromMinutes(5))
            .PostMultipartAsync(mp =>
            {
                mp.AddFile("File", new MemoryStream(memoryStream.ToArray()), "file.zip");
                mp.AddString("ServiceName", service!.Name);
                mp.AddString("ConnectionId", connection?.ConnectionId ?? "");
                mp.AddString("Params", request.ExeParams ?? "");
                mp.AddString("ExeFullPath", Path.Combine(request.TargetDir, request.ExePath) ?? "");
                mp.AddString("ServiceDescription", request.Description ?? "");
            })
            .ReceiveJson<UnifyResult<object>>();
        if (!resp.success)
        {
            Log.Error(resp.ToJsonString());
            NotifyService.NotifyMessageToUser($"安装失败 {resp.msg}");
        }
    }

    public async Task UpdateAgent(string? zipFilePath, DeployTarget deployTarget)
    {
        try
        {
            await GetHttpClient("api/deploy/updateself")
                .PostMultipartAsync(content => { content.AddFile("file", zipFilePath); });
        }
        catch (FlurlHttpException ex)
        {
            var body = await ex.GetResponseStringAsync();
            NotifyService.NotifyMessageToUser("处理异常");
            NotifyService.NotifyMessageToUser($"返回消息 {ex.Message}");
            NotifyService.NotifyMessageToUser($"返回消息 {body}");
        }
    }
    
    public async Task CopyFile(string zipFilePath,string targetDirPath, DeployTarget deployTarget)
    {
        try
        {
            await GetHttpClient("api/deploy/updateself")
                .PostMultipartAsync(content =>
                {
                    content.AddFile("file", zipFilePath);
                    content.AddString("targetDir", targetDirPath);
                });
        }
        catch (FlurlHttpException ex)
        {
            var body = await ex.GetResponseStringAsync();
            NotifyService?.NotifyMessageToUser("处理异常");
            NotifyService?.NotifyMessageToUser($"返回消息 {ex.Message}");
            NotifyService?.NotifyMessageToUser($"返回消息 {body}");
        }
    }

    public async Task<string?> GetStatus(string serviceName)
    {
        var response = await GetHttpClient("api/deploy/getstatus")
            .WithTimeout(3)
            .SetQueryParam("serviceName", serviceName)
            .GetJsonAsync<UnifyResult<string>>();
        if (response.success)
        {
            return response.data??string.Empty;
        }

        NotifyService.NotifyMessageToUser(response.msg??"获取状态失败");
        return response.msg??"获取状态失败";
    }
    public async Task<string?> GetAgentVersion(DeployTarget target)
    {
        _target = target;
        var response = await GetHttpClient("api/deploy/getagentversion")
            .WithTimeout(1)
            .GetJsonAsync<UnifyResult<string>>();
        if (response.success)
        {
            return response.data??string.Empty;
        }
        return response.msg??"获取版本失败";
    }


    public async Task<List<FileHelper.FileInfoDto>?> CompareInDir(List<FileHelper.FileInfoDto> currentFileInfos, string targetDir,CancellationToken cancellationToken)
    {
        var result = await GetHttpClient("api/deploy/comparedir")
            .SetQueryParam("targetDir", targetDir)
            .PostJsonAsync(currentFileInfos,cancellationToken)
            .ReceiveJson<UnifyResult<List<FileHelper.FileInfoDto>>>();
        
        if (result.success)
        {
            return result.data;
        }
        else
        {
            NotifyService.NotifyMessageToUser(result.msg??string.Empty);
            return null;
        }
    }
}