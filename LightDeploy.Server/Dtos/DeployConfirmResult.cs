
namespace LightDeploy.Server.Dtos
{
    public class DeployConfirmResult
    {
        /// <summary>
        /// 发布消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 是否使用快速对比模式，仅比较文件字节大小
        /// </summary>
        public bool UseFastMode { get; set; }

        /// <summary>
        /// 是否通知企业微信
        /// </summary>
        public bool NotifyWeChat { get; set; }
    }
}


