using Flurl.Http;
using Flurl.Http.Configuration;
using LightApi.Infra.Unify;
using LightDeploy.Core;
using LightDeploy.Core.Autofac;
using LightDeploy.Core.Extension;
using LightDeploy.Core.Helper;
using LightDeploy.Domain;
using LightDeploy.Service;
using LightDeploy.Service.Dto;
using Masuit.Tools;
using Microsoft.AspNetCore.SignalR.Client;
using Nacos.V2.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;

namespace LightDeployApp.Services;

public class AgentService : ITransientDependency, IAsyncDisposable
{
    private readonly NotifyService _notifyService;
    private HubConnection? connection;

    private DeployTarget _target;

    public AgentService(NotifyService notifyService)
    {
        _notifyService = notifyService;
    }

    public async Task InitTargetConnection(DeployTarget target)
    {
        if (connection == null)
        {
            _target = target;
            connection = new HubConnectionBuilder()
                .WithUrl($"http://{target.Host}:{target.Port}/agent")
                .WithAutomaticReconnect()
                .Build();

            _notifyService.SetHost(target.Host);
            connection.On("Log", (string msg) => { _notifyService.NotifyMessageToUser($"Agent消息: " + msg); });
            await connection.StartAsync();
        }
    }

    /// <summary>
    /// 获取HTTP连接
    /// </summary>
    /// <param name="target"></param>
    /// <param name="endpoint"></param>
    /// <returns></returns>
    private IFlurlRequest GetHttpClient(string endpoint)
    {
        var client = $"http://{_target.Host}:{_target.Port}/{endpoint}"
                .WithHeader("Authorization", _target.AuthKey ?? "")
                .WithHeader("WsConnectionId", connection.ConnectionId)
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
        CancellationToken cancellationToken = default)
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
            await _notifyService.NotifyMessageToUser(result.msg);
            return null;
        }
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="memoryStream"></param>
    /// <param name="serviceName"></param>
    /// <param name="calculateNeedDeployFiles"></param>
    /// <returns></returns>
    public async Task<bool> Upload(Stream memoryStream, string serviceName,
        List<FileHelper.FileInfoDto> calculateNeedDeployFiles,string targetDir,bool onlyCopyFiles)
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
                    _target.Service!.EnableHealthCheck && _target.HealthCheckUrl.IsNotNullOrWhiteSpace()
                        ? _target.HealthCheckUrl
                        : "");
            })
            .ReceiveJson<UnifyResult<object>>();
        if (!response.success)
        {
            await _notifyService.NotifyMessageToUser($"部署失败{_target.Host}:{_target.Port} {response.msg}");
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
            await _notifyService.NotifyMessageToUser(response.msg ?? "启动失败");
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
            await _notifyService.NotifyMessageToUser(response.msg ?? "停止失败");
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
            await _notifyService.NotifyMessageToUser(response.msg ?? "获取状态失败");
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
                mp.AddString("ServiceName", deployTarget.Service.Name);
                mp.AddString("ConnectionId", connection?.ConnectionId ?? "");
                mp.AddString("Params", request.ExeParams ?? "");
                mp.AddString("ExeFullPath", Path.Combine(request.TargetDir, request.ExePath) ?? "");
                mp.AddString("ServiceDescription", request.Description ?? "");
            })
            .ReceiveJson<UnifyResult>();
        if (!resp.success)
        {
            Log.Error(resp.ToJsonString());
            await _notifyService.NotifyMessageToUser($"安装失败 {resp.msg}");
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
            await _notifyService.NotifyMessageToUser("处理异常");
            await _notifyService.NotifyMessageToUser($"返回消息 {ex.Message}");
            await _notifyService.NotifyMessageToUser($"返回消息 {body}");
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

        await _notifyService.NotifyMessageToUser(response.msg??"获取状态失败");
        return null;
    }


    public async Task<List<FileHelper.FileInfoDto>> CompareInDir(List<FileHelper.FileInfoDto> currentFileInfos, string targetDir)
    {
        var result = await GetHttpClient("api/deploy/comparedir")
            .SetQueryParam("targetDir", targetDir)
            .PostJsonAsync(currentFileInfos)
            .ReceiveJson<UnifyResult<List<FileHelper.FileInfoDto>>>();
        
        if (result.success)
        {
            return result.data;
        }
        else
        {
            await _notifyService.NotifyMessageToUser(result.msg);
            return null;
        }
    }
}