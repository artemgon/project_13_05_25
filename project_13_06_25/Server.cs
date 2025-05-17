using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UdpHelpers;

namespace UdpServer
{
    public class Server
    {
        private readonly SocketConfig _config;
        private UdpClient _server;

        public Server(SocketConfig config)
        {
            _config = config;
            _server = new UdpClient(_config.Port);
        }

        public void Start()
        {
            Console.WriteLine($"Server started on port {_config.Port}");

            try
            {
                while (true)
                {
                    IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = _server.Receive(ref clientEndPoint);
                    string message = Encoding.UTF8.GetString(data);

                    Console.WriteLine($"Received from {clientEndPoint}: {message}");

                    byte[] response = Encoding.UTF8.GetBytes($"ECHO: {message}");
                    _server.Send(response, response.Length, clientEndPoint);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Socket error: {ex.Message}");
            }
            finally
            {
                _server.Close();
            }
        }
    }
}
