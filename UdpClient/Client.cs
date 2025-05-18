using System.Net;
using System.Net.Sockets;
using System.Text;
using UdpHelpers;

namespace UdpClient
{
    public class Client : IDisposable
    {
        private readonly SocketConfig _config;
        private readonly System.Net.Sockets.UdpClient _udpClient; 
        private bool _isRunning = true;

        public Client(SocketConfig config)
        {
            _config = config;
            _udpClient = new System.Net.Sockets.UdpClient(); 
            _udpClient.Client.ReceiveTimeout = 3000;
        }

        public void Run()
        {
            var serverEP = new IPEndPoint(
                IPAddress.Parse(_config.DefaultServerIP),
                _config.Port
            );

            Console.WriteLine("UDP Client (press ENTER to exit)");
            Console.WriteLine("Select the component:");
            Console.WriteLine("1. CPU");
            Console.WriteLine("2. GPU");
            Console.WriteLine("3. RAM");

            Task.Run(() =>
            {
                while (_isRunning)
                {
                    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Enter)
                    {
                        _isRunning = false;
                        Console.WriteLine("\nExiting...");
                        break;
                    }
                    Thread.Sleep(100);
                }
            });

            while (_isRunning)
            {
                Console.Write("> ");
                string input = GetUserInput();
                if (!_isRunning || string.IsNullOrEmpty(input)) break;

                string component = input switch
                {
                    "1" => "CPU",
                    "2" => "GPU",
                    "3" => "RAM",
                    _ => input.ToUpper()
                };

                byte[] datagram = Encoding.UTF8.GetBytes(component);
                _udpClient.Send(datagram, datagram.Length, serverEP);

                try
                {
                    byte[] response = _udpClient.Receive(ref serverEP);
                    string responseText = Encoding.UTF8.GetString(response);
                    if (responseText.StartsWith("ERROR:"))
                    {
                        Console.WriteLine($"Server rejected request: {responseText}");
                        if (responseText.Contains("limit exceeded"))
                        {
                            Thread.Sleep(5000);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Price: {responseText}");
                    }
                }
                catch (SocketException)
                {
                    Console.WriteLine("No response from server (timeout)");
                }
            }
        }

        private string GetUserInput()
        {
            StringBuilder input = new StringBuilder();
            while (_isRunning)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return input.ToString();
                }
                else if (key.Key == ConsoleKey.Escape)
                {
                    _isRunning = false;
                    return string.Empty;
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    Console.Write(key.KeyChar);
                    input.Append(key.KeyChar);
                }
            }
            return string.Empty;
        }

        public void Dispose()
        {
            _isRunning = false;
            _udpClient?.Close();
        }
    }
}