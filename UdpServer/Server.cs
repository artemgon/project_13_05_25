using System;
using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<IPAddress, ClientTracker> _clientTrackers = new();
        private readonly int _maxQueriesPerHour = 10;
        private readonly SemaphoreSlim _clientSemaphore;
        private readonly int _maxClients = 5;
        private readonly ConcurrentDictionary<string, string> _dataStore = new();
        private readonly SocketConfig _config;
        private readonly UdpClient _udpServer;
        private readonly CancellationTokenSource _cts = new();
        private readonly ConcurrentDictionary<IPEndPoint, DateTime> _lastActivityTimes = new();

        class ClientTracker
        {
            public int QueryCount;
            public DateTime WindowStart = DateTime.UtcNow;
        }

        public Server(SocketConfig config)
        {
            _clientSemaphore = new SemaphoreSlim(_maxClients, _maxClients);
            _config = config;
            _udpServer = new UdpClient(_config.Port);

            _dataStore.TryAdd("CPU", "10000");
            _dataStore.TryAdd("GPU", "20000");
            _dataStore.TryAdd("RAM", "4000");
        }

        public async Task StartAsync()
        {
            Console.WriteLine($"Server started on port {_config.Port}");

            var cleanupTask = CleanInactiveClientsAsync();

            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    var result = await _udpServer.ReceiveAsync(_cts.Token);
                    UpdateActivity(result.RemoteEndPoint);
                    _ = HandleClientAsync(result.RemoteEndPoint, result.Buffer);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("\nShutdown requested. Completing pending requests...");
                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server error: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Server resources released");
                await cleanupTask;
                _udpServer.Close();
            }
        }

        private void UpdateActivity(IPEndPoint clientEndPoint)
        {
            _lastActivityTimes.AddOrUpdate(
                clientEndPoint,
                DateTime.UtcNow,
                (_, _) => DateTime.UtcNow
            );
        }
        private async Task CleanInactiveClientsAsync()
        {
            while (!_cts.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(1)); 

                var cutoff = DateTime.UtcNow.AddMinutes(-10);
                var inactiveClients = _lastActivityTimes
                    .Where(kvp => kvp.Value < cutoff)
                    .ToList();

                foreach (var client in inactiveClients)
                {
                    _lastActivityTimes.TryRemove(client.Key, out _);
                    Console.WriteLine($"Disconnected inactive client: {client.Key}");
                }
            }
        }

        private bool IsRateLimited(IPAddress clientIp)
        {
            var tracker = _clientTrackers.GetOrAdd(clientIp, _ => new ClientTracker());

            if ((DateTime.UtcNow - tracker.WindowStart).TotalHours >= 1)
            {
                tracker.QueryCount = 0;
                tracker.WindowStart = DateTime.UtcNow;
            }

            if (++tracker.QueryCount > _maxQueriesPerHour)
            {
                return true;
            }
            return false;
        }

        private async Task HandleClientAsync(IPEndPoint clientEndPoint, byte[] requestData)
        {
            if (IsRateLimited(clientEndPoint.Address))
            {
                await _udpServer.SendAsync(Encoding.UTF8.GetBytes("ERROR: Hourly query limit exceeded"), clientEndPoint);
                return;
            }
            if (!await _clientSemaphore.WaitAsync(0))
            {
                await _udpServer.SendAsync(Encoding.UTF8.GetBytes("ERROR: Server busy"), clientEndPoint);
                return;
            }
            try
            {
                if (_lastActivityTimes.TryGetValue(clientEndPoint, out var lastActive) &&
                    lastActive < DateTime.UtcNow.AddMinutes(-10))
                {
                    Console.WriteLine($"Previously inactive client reconnected: {clientEndPoint}");
                }

                string requestedComponent = Encoding.UTF8.GetString(requestData).Trim();
                string response = _dataStore.TryGetValue(requestedComponent, out string? price)
                    ? price
                    : "ERROR: Component not available";

                Console.WriteLine($"Handling request from {clientEndPoint}: {requestedComponent}");
                await _udpServer.SendAsync(Encoding.UTF8.GetBytes(response), clientEndPoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client {clientEndPoint}: {ex.Message}");
            }
            finally
            {
                _clientSemaphore.Release();
            }
        }

        public void Stop()
        {
            _cts.Cancel();
        }
    }
}
