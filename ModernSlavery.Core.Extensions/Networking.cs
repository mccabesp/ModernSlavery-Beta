using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
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

        public static bool IsTrustedAddress(this string testIPAddress, params string[] trustedHostnamesOrIpAddressesORCidrs)
        {
            if (trustedHostnamesOrIpAddressesORCidrs==null || !trustedHostnamesOrIpAddressesORCidrs.Any()) throw new ArgumentNullException(nameof(trustedHostnamesOrIpAddressesORCidrs));

            return trustedHostnamesOrIpAddressesORCidrs.Any(trustedHostnameOrIpAddressORCidr => IsTrustedAddress(testIPAddress, trustedHostnameOrIpAddressORCidr));
        }

        private static bool IsTrustedAddress(this string testIPAddress, string trustedHostnameOrIpAddressORCidr)
        {
            if (string.IsNullOrWhiteSpace(testIPAddress)) throw new ArgumentNullException(nameof(testIPAddress));
            if (!IPAddress.TryParse(testIPAddress, out var testIP)) throw new ArgumentException($"Invalid IP Address '{testIPAddress}'", nameof(testIPAddress));
            if (string.IsNullOrWhiteSpace(trustedHostnameOrIpAddressORCidr)) throw new ArgumentNullException(nameof(trustedHostnameOrIpAddressORCidr));

            //Check if IP on local subnet
            if (testIP.IsOnLocalSubnet()) return true;

            if (IPAddress.TryParse(trustedHostnameOrIpAddressORCidr, out var hostIP))
            {
                //Check for exact IP match
                return hostIP.Equals(testIP);
            }
            else if (IsValidCIDR(trustedHostnameOrIpAddressORCidr))
            {
                //Check for CIDR match
                var ipNetwork = CIDRToIPNetwork(trustedHostnameOrIpAddressORCidr);
                return ipNetwork.Contains(testIP);
            }
            else if (IsValidHostname(trustedHostnameOrIpAddressORCidr))
            {
                //Check for domain IP match
                var retryPolicy = Resilience.GetExponentialRetryPolicy<SocketException>(5,filter: sex => sex.Message.Contains("No such host is known.", StringComparison.OrdinalIgnoreCase));
                var trustedIPs = retryPolicy.Execute(() => Dns.GetHostAddresses(trustedHostnameOrIpAddressORCidr));
                return trustedIPs != null && trustedIPs.Any(address => address.Equals(testIP));
            }
            else
                throw new ArgumentOutOfRangeException(nameof(trustedHostnameOrIpAddressORCidr), trustedHostnameOrIpAddressORCidr, "Not a valid IP Address, Hostname or CIDR");
        }

        public static bool IsValidHostname(this string hostname)
        {
            return Uri.CheckHostName(hostname)!=UriHostNameType.Unknown;
        }

        public static bool IsValidCIDR(this string cidr)
        {
            var subnet = cidr.BeforeFirst("/");
            var bits= cidr.AfterFirst("/");
            if (string.IsNullOrWhiteSpace(subnet) || !IPAddress.TryParse(subnet, out var testIP)) return false;
            if (string.IsNullOrWhiteSpace(bits) || !byte.TryParse(bits, out var mask) || mask>32) return false;

            return true;
        }

        public static IPNetwork CIDRToIPNetwork(string cidr)
        {
            if (string.IsNullOrWhiteSpace(cidr)) throw new ArgumentNullException(nameof(cidr));
            var seperatorIndex = cidr.IndexOf('/');

            IPAddress ipAddress;
            byte prefixLength = 32;

            if (seperatorIndex < 0)
                ipAddress = IPAddress.Parse(cidr);
            else
            {
                prefixLength = byte.Parse(cidr.Substring(seperatorIndex+1));
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
                    var parts = response.Split(':');
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
    }
}