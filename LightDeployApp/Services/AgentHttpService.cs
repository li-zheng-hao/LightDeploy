using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using LightDeployApp.Dtos;
using LightDeployApp.Tables;
using Masuit.Tools;
using Microsoft.AspNetCore.SignalR.Client;

namespace LightDeployApp.Services;

public class AgentHttpService:IAsyncDisposable
{
    private SelectedEnvironment environment;
    private readonly HubConnection connection;

    public AgentHttpService(SelectedEnvironment environment)
    {
        this.environment = environment;
        connection = new HubConnectionBuilder()
            .WithUrl($"http://{environment.Host}:{environment.Port}/agent")
            .WithAutomaticReconnect()
            .Build();
        connection.On("Log", (string msg) => { AppContext.GetAppDataContext().Log($"Agent消息: " + msg); });
        connection.StartAsync().GetAwaiter().GetResult();
    }
    private  IFlurlRequest GetHttpClient(string subUrl)
    {
        return $"http://{environment.Host}:{environment.Port}/{subUrl}"
            .WithHeader("Authorization", environment.AuthKey ?? "")
            .WithHeader("WsConnectionId", connection.ConnectionId)
            ;
    }
    /// <summary>
    /// 对比文件
    /// </summary>
    /// <param name="source"></param>
    /// <param name="serviceName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<List<FileInfoDto>?> Compare(List<FileInfoDto> source,string serviceName,CancellationToken cancellationToken=default)
    {
        var result =await GetHttpClient("api/deploy/compare")
            .SetQueryParam("serviceName", serviceName)
            .WithHeader("Authorization", environment.AuthKey ?? "")
            .PostJsonAsync(source, cancellationToken)
            .ReceiveJson<UnifyResult<List<FileInfoDto>>>();
        if (result.Success)
        {
            return result.Data;
        }
        else
        {
            AppContext.Log(result.Msg);
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
    public async Task<bool> Upload(Stream memoryStream,string serviceName,List<FileInfoDto> calculateNeedDeployFiles)
    {
        var response = await GetHttpClient("api/deploy/deploy")
            .PostMultipartAsync(mp =>
            {
                mp.AddFile("File", new MemoryStream(memoryStream.ToArray()), "file.zip");
                mp.AddString("ServiceName", serviceName);
                mp.AddJson("FileInfos", calculateNeedDeployFiles);
                mp.AddString("ConnectionId", connection?.ConnectionId ?? "");
                mp.AddString("HealthCheckUrl", environment.HealthCheckUrl ?? "");
            })
            .ReceiveJson<UnifyResult<object>>();
        if (!response.Success)
        {
            AppContext.GetAppDataContext().Log($"部署失败{environment.Host}:{environment.Port} {response.Msg}");
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
        if (!response.Success)
        {
            AppContext.Log(response.Msg);
        }
    }
    public async Task StopService(string serviceName)
    {
        var response = await GetHttpClient("api/deploy/stopservice")
            .SetQueryParam("serviceName", serviceName)
            .PostAsync()
            .ReceiveJson<UnifyResult<object>>();
        if (!response.Success)
        {
            AppContext.Log(response.Msg);
        }
    }

    public async Task<string?> RefreshService(string serviceName)
    {
        var response = await GetHttpClient("api/deploy/getstatus")
            .WithTimeout(5)
            .WithHeader("Authorization", environment.AuthKey ?? "")
            .SetQueryParam("serviceName", serviceName)
            .GetJsonAsync<UnifyResult<string>>();
        if (response.Success)
        {
            return response.Data??string.Empty;
        }
        else
        {
            AppContext.Log(response.Msg);
            return default;
        }
        
    }
    public async ValueTask DisposeAsync()
    {
        await connection.DisposeAsync();
    }
    
    
}