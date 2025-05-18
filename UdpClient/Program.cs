using UdpHelpers;

namespace UdpClient
{
    public class Program
    {
        static void Main()
        {
            var config = new SocketConfig();
            using var client = new Client(config);
            client.Run();
        }
    }
}
