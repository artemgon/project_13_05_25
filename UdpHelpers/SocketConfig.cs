using Shared;

namespace UdpHelpers
{
    public class SocketConfig
    {
        public int Port { get; set; }
        public string DefaultServerIP { get; set; } = Constants.DefaultServerIp;
        public int BufferSize { get; set; } = Constants.BufferSize;
        public int MaxClients { get; set; } = 50;
        public int MaxQueriesPerHour { get; set; } = 10;
    }
}
