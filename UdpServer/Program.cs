using UdpHelpers;

namespace UdpServer
{
    public class Program
    {
        static void Main()
        {
            var config = new SocketConfig { Port = 11000 };
            var server = new Server(config);
            server.Start();
        }
    }
}
