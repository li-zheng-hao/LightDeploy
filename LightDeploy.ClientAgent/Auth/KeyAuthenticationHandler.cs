using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace LightDeploy.ClientAgent.Auth;

public class KeyAuthenticationHandler : AuthenticationHandler<KeyAuthenticationOptions>
{
    private readonly IConfiguration _configuration;

    public KeyAuthenticationHandler(IOptionsMonitor<KeyAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock,IConfiguration configuration)
        : base(options, logger, encoder, clock)
    {
        _configuration = configuration;
    }
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (string.IsNullOrWhiteSpace(_configuration["AuthKey"]))
        {
            var identity = new ClaimsIdentity(null, KeyAuthenticationOptions.Scheme);
            var identities = new List<ClaimsIdentity> { identity };
            var principal = new ClaimsPrincipal(identities);
            var ticket = new AuthenticationTicket(principal, KeyAuthenticationOptions.Scheme);
 
            return AuthenticateResult.Success(ticket);
        }
        
        var authKey=Request.Headers.Authorization.ToString();
        
        if (_configuration["AuthKey"]==authKey)
        {
            var identity = new ClaimsIdentity(null, KeyAuthenticationOptions.Scheme);
            var identities = new List<ClaimsIdentity> { identity };
            var principal = new ClaimsPrincipal(identities);
            var ticket = new AuthenticationTicket(principal, KeyAuthenticationOptions.Scheme);
 
            return AuthenticateResult.Success(ticket);
        }
 
        return AuthenticateResult.Fail("Unauthorized");
    }
}