using Lib.AspNetCore.ServerSentEvents;
using Microsoft.AspNetCore.Http;

namespace LightDeploy.Core.Sse;

public class NewGuidServerSentEventsClientIdProvider : IServerSentEventsClientIdProvider
{
    public Guid AcquireClientId(HttpContext context)
    {
        return Guid.NewGuid();
    }

    public void ReleaseClientId(Guid clientId, HttpContext context)
    {
    }
}