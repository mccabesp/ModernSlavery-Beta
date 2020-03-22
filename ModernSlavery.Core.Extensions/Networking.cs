using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace ModernSlavery.Core.Extensions
{
    public static class Networking
    {
        private static IPAddress GetNetworkAddress(this IPAddress address, IPAddress subnetMask)
        {
            var ipAdressBytes = address.GetAddressBytes();
            var subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            var broadcastAddress = new byte[ipAdressBytes.Length];
            for (var i = 0; i < broadcastAddress.Length; i++)
                broadcastAddress[i] = (byte) (ipAdressBytes[i] & subnetMaskBytes[i]);

            return new IPAddress(broadcastAddress);
        }

        private static bool IsInSameSubnet(this IPAddress address2, IPAddress address, IPAddress subnetMask)
        {
            var network1 = address.GetNetworkAddress(subnetMask);
            var network2 = address2.GetNetworkAddress(subnetMask);

            return network1.Equals(network2);
        }

        [DebuggerStepThrough]
        private static bool IsOnLocalSubnet(this string hostName)
        {
            if (string.IsNullOrWhiteSpace(hostName)) throw new Exception("Missing hostname");

            try
            {
                var IPs = Dns.GetHostAddresses(hostName);
                if (IPs == null || IPs.Length < 1)
                    throw new Exception("Could not resolve host name '" + hostName + "'");

                foreach (var address in IPs)
                    if (address.IsOnLocalSubnet())
                        return true;
            }
            catch
            {
            }

            return false;
        }

        public static bool IsTrustedAddress(this string hostName, string[] trustedIPdomains)
        {
            if (string.IsNullOrWhiteSpace(hostName)) throw new ArgumentNullException(nameof(hostName));

            if (trustedIPdomains == null || trustedIPdomains.Length == 0)
                throw new ArgumentNullException(nameof(trustedIPdomains));

            if (trustedIPdomains.ContainsI(hostName)) return true;

            try
            {
                var IPs = Dns.GetHostAddresses(hostName);
                if (IPs == null || IPs.Length < 1)
                    throw new Exception("Could not resolve host name '" + hostName + "'");

                if (IPs.Any(address => trustedIPdomains.ContainsI(address.ToString()) || address.IsOnLocalSubnet()))
                    return true;
            }
            catch
            {
            }

            return hostName.IsOnLocalSubnet();
        }

        private static bool IsOnLocalSubnet(this IPAddress clientIP)
        {
            if (clientIP.ToString().EqualsI("::1", "127.0.0.1")) return true;

            foreach (var networkAdapter in NetworkInterface.GetAllNetworkInterfaces())
                if (networkAdapter.OperationalStatus == OperationalStatus.Up)
                {
                    var interfaceProperties = networkAdapter.GetIPProperties();
                    var IPsettings = interfaceProperties.UnicastAddresses;
                    foreach (var IPsetting in IPsettings)
                    {
                        if (clientIP.Equals(IPsetting.Address)) return true;

                        if (IPsetting.IPv4Mask == null || IPsetting.IPv4Mask.ToString() == "0.0.0.0") continue;

                        if (clientIP.IsInSameSubnet(IPsetting.Address, IPsetting.IPv4Mask)) return true;
                    }
                }

            return false;
        }
    }
}