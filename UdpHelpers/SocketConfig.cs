using Shared;

namespace UdpHelpers
{
    public class SocketConfig
    {
        public int Port { get; set; } = Constants.DefaultPort;
        public string DefaultServerIP { get; set; } = Constants.DefaultServerIp;
    }
}
