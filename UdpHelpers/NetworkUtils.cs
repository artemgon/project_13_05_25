using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace UdpHelpers
{
    public static class NetworkUtils
    {
        public static IPAddress GetLocalIPv4(NetworkInterfaceType type = NetworkInterfaceType.Ethernet)
        {
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == type && item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return ip.Address;
                        }
                    }
                }
            }
            return IPAddress.Loopback;
        }

        public static bool TryParseEndpoint(string input, int defaultPort, out IPEndPoint? endpoint)
        {
            endpoint = null;

            try
            {
                string[] parts = input.Split(':');
                IPAddress ip = IPAddress.Parse(parts[0]);
                int port = parts.Length > 1 ? int.Parse(parts[1]) : defaultPort;

                endpoint = new IPEndPoint(ip, port);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsPortAvailable(int port)
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                try
                {
                    socket.Bind(new IPEndPoint(IPAddress.Any, port));
                    return true;
                }
                catch (SocketException)
                {
                    return false;
                }
            }
        }

        public static IPAddress GetBroadcastAddress(IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Address and mask length mismatch");

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));
            }
            return new IPAddress(broadcastAddress);
        }

        public static bool IsValidMulticastAddress(IPAddress address)
        {
            if (address.AddressFamily != AddressFamily.InterNetwork)
                return false;

            byte[] bytes = address.GetAddressBytes();
            return bytes[0] >= 224 && bytes[0] <= 239;
        }

        public static void ListNetworkInterfaces()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                Console.WriteLine($"{ni.Name} ({ni.NetworkInterfaceType}): {ni.OperationalStatus}");

                foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        Console.WriteLine($"  {ip.Address}/{ip.PrefixLength}");
                    }
                }
            }
        }
    }
}
