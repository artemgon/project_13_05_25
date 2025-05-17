using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System;
using UdpHelpers;

namespace UdpClient
{
    public class Client
    {
        private readonly SocketConfig _config;
        private System.Net.Sockets.UdpClient _client; 

        public Client(SocketConfig config)
        {
            _config = config;
            _client = new System.Net.Sockets.UdpClient();
        }

        public void Run()
        {
            Console.Write("Enter server IP [127.0.0.1]: ");
            string serverIp = Console.ReadLine() ?? "";
            if (string.IsNullOrEmpty(serverIp))
            {
                serverIp = "127.0.0.1";
            }

            var serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), _config.Port);

            Console.WriteLine($"UDP client ready to send to {serverEndPoint}");
            Console.WriteLine("Type messages to send (empty to exit)");

            try
            {
                while (true)
                {
                    Console.Write("> ");
                    string message = Console.ReadLine() ?? "";

                    if (string.IsNullOrEmpty(message))
                    {
                        break;
                    }

                    SendMessage(serverEndPoint, message);
                    ReceiveResponse(ref serverEndPoint);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                _client.Close();
            }
        }

        private void SendMessage(IPEndPoint endpoint, string message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            _client.Send(bytes, bytes.Length, endpoint);
            Console.WriteLine($"Sent: {message}");
        }

        private void ReceiveResponse(ref IPEndPoint endpoint)
        {
            byte[] response = _client.Receive(ref endpoint);
            Console.WriteLine($"Received: {Encoding.UTF8.GetString(response)}");
        }
    }
}
