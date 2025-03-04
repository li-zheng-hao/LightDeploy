using System;

namespace LightDeploy.Server.Dtos;

public class DeployConfirmResult
{
    public string ReleaseNote { get; set; }
    public bool NotifyWeChat { get; set; }
}
