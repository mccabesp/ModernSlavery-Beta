using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.AspNetCore.HttpOverrides;

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

        public static bool IsTrustedAddress(this string testIPAddress, string[] trustedIPdomains)
        {
            if (string.IsNullOrWhiteSpace(testIPAddress)) throw new ArgumentNullException(nameof(testIPAddress));

            if (trustedIPdomains == null || trustedIPdomains.Length == 0)
                throw new ArgumentNullException(nameof(trustedIPdomains));

            return trustedIPdomains.Any(trustedHostnameOrIpAddressORCidr => IsTrustedAddress(testIPAddress, trustedHostnameOrIpAddressORCidr));
        }

        public static bool IsTrustedAddress(this string testIPAddress, string trustedHostnameOrIpAddressORCidr)
        {
            if (string.IsNullOrWhiteSpace(trustedHostnameOrIpAddressORCidr)) throw new ArgumentNullException(nameof(trustedHostnameOrIpAddressORCidr));
            if (string.IsNullOrWhiteSpace(testIPAddress)) throw new ArgumentNullException(nameof(testIPAddress));
            if (!IPAddress.TryParse(testIPAddress, out IPAddress testIP)) throw new ArgumentException($"Invalid IP Address '{testIPAddress}'",nameof(testIPAddress));

            //Check if IP on local subnet
            if (testIP.IsOnLocalSubnet()) return true;

            //Check for exact domain or IP match
            if (trustedHostnameOrIpAddressORCidr.Equals(testIPAddress,StringComparison.OrdinalIgnoreCase)) return true;

            //Check for CIDR match
            if (trustedHostnameOrIpAddressORCidr.IndexOf('/') > -1)
            {
                var ipNetwork = CIDRToIPNetwork(trustedHostnameOrIpAddressORCidr);
                return ipNetwork.Contains(testIP);
            }

            //Check for domain IP match
            var trustedIPs = Dns.GetHostAddresses(trustedHostnameOrIpAddressORCidr);
            return trustedIPs != null && trustedIPs.Any(address => address.Equals(testIP));
        }

        public static IPNetwork CIDRToIPNetwork(string cidr)
        {
            if (string.IsNullOrWhiteSpace(cidr)) throw new ArgumentNullException(nameof(cidr));
            var seperatorIndex = cidr.IndexOf('/');

            IPAddress ipAddress;
            var prefixLength = 32;

            if (seperatorIndex < 0)
                ipAddress = IPAddress.Parse(cidr);
            else
            {
                prefixLength = int.Parse(cidr.Substring(seperatorIndex+1));
                ipAddress = IPAddress.Parse(cidr.Substring(0, seperatorIndex));
            }
            return new IPNetwork(ipAddress, prefixLength);
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

        public static async Task<string> GetEgressIPAddressAsync()
        {
            string url;
            string response;

            try
            {
                url = "http://ipinfo.io/ip";
                response = await Web.WebRequestAsync(Web.HttpMethods.Get, url).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(response) && !response.StartsWith("127.0.0.1"))
                {
                    return response.Trim();
                }
            }
            catch { }

            try
            {
                url = "https://myexternalip.com/raw";
                response = await Web.WebRequestAsync(Web.HttpMethods.Get,url).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(response) && !response.StartsWith("127.0.0.1"))
                {
                    return response.Trim();
                }
            }
            catch { }

            try
            {
                url = "http://ipv4bot.whatismyipaddress.com/";
                response = await Web.WebRequestAsync(Web.HttpMethods.Get, url).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(response) && !response.StartsWith("127.0.0.1"))
                {
                    return response.Trim();
                }
            }
            catch { }

            try
            {
                url = "http://checkip.dyndns.org";
                response = await Web.WebRequestAsync(Web.HttpMethods.Get, url).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(response))
                {
                    string[] parts = response.Split(':');
                    response = parts[1].Substring(1);
                    parts = response.Split('<');
                    response = parts[0];
                }

                if (!string.IsNullOrWhiteSpace(response) && !response.StartsWith("127.0.0.1"))
                {
                    return response.Trim();
                }
            }
            catch { }

            return null;
        }

        /// <summary>
        /// Setup a connection lease for HttpClient
        /// </summary>
        /// <param name="httpClient">The HttpClient to setup the connection lease</param>
        /// <param name="baseAddress">the base address of Uniform Resource Identifier (URI) of the Internet resource used when sending requests</param>
        /// <param name="connectionLeaseTimeout">The number of milliseconds after which an active System.Net.ServicePoint connection is closed. Defaults to 60 Seconds</param>
        public static void SetupConnectionLease(this HttpClient httpClient, string baseAddress, int connectionLeaseTimeout=60000)
        {
            if (string.IsNullOrWhiteSpace(baseAddress)) throw new ArgumentNullException(nameof(baseAddress));
            httpClient.BaseAddress = new Uri(baseAddress);
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.ConnectionClose = false;
            ServicePointManager.FindServicePoint(httpClient.BaseAddress).ConnectionLeaseTimeout = connectionLeaseTimeout;
        }
    }
}