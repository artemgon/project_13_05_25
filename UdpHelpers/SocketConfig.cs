namespace UdpHelpers
{
    public class SocketConfig
    {
        public int Port { get; set; } = 11000;
        public string LocalIP { get; set; } = "0.0.0.0";
        public int TimeoutMS { get; set; } = 2000;
        public int BufferSize { get; set; } = 1024;
    }
}
