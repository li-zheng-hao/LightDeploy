using Microsoft.AspNetCore.Authentication;

namespace LightDeploy.ClientAgent.Auth;

public class KeyAuthenticationOptions:AuthenticationSchemeOptions
{
    
    public const string Scheme = "Key";
}