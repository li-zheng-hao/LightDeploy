using Microsoft.AspNetCore.Authentication;

namespace LightDeploy.ClientAgent.Auth;

public static class AuthenticationBuilderExtensions
{
    public static AuthenticationBuilder AddKeyAuthentication(this AuthenticationBuilder authenticationBuilder, Action<KeyAuthenticationOptions> options)
    {
        return authenticationBuilder.AddScheme<KeyAuthenticationOptions, KeyAuthenticationHandler>(KeyAuthenticationOptions.Scheme, options);
    }
}