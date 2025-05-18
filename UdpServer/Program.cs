using UdpHelpers;
using Shared;

namespace UdpServer
{
    public class Program
    {
        public static async Task Main()
        {
            var config = new SocketConfig { Port = Constants.DefaultPort };
            var server = new Server(config);

            var serverTask = server.StartAsync();

            Console.WriteLine("Press Q to stop server...");
            while (Console.ReadKey().Key != ConsoleKey.Q) { }

            server.Stop();
            await serverTask;
        }
    }
}
