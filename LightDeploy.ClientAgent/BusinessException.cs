namespace LightDeploy.ClientAgent;

public class BusinessException:Exception
{
    public BusinessException(string message) : base(message)
    {
    }
}