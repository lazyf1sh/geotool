using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GeoTool
{

    public static class IpUtilities
    {
        public static uint IpToUint(IPAddress ip)
        {
            return (((uint)ip.GetAddressBytes()[0] << 24) | ((uint)ip.GetAddressBytes()[1] << 16) | ((uint)ip.GetAddressBytes()[2] << 8) | ip.GetAddressBytes()[3]);
        }

        public static IPAddress UintToIp(uint ipAddress)
        {
            IPAddress tmpIp;
            if (IPAddress.TryParse(ipAddress.ToString(), out tmpIp))
            {
                try
                {
                    Byte[] bytes = tmpIp.GetAddressBytes();
                    long addr = (long)BitConverter.ToInt32(bytes, 0);
                    return new IPAddress(addr);
                }
                catch (Exception) { return null; }
            }
            else return null;
        }

        private static IPRange CalculateIpRange(string cidrSubnet)
        {
            IPAddress ipAddress = IPAddress.Parse(cidrSubnet.Split('/')[0]);
            int bits = int.Parse(cidrSubnet.Split('/')[1]);

            uint mask = ~(uint.MaxValue >> bits);

            byte[] ipBytes = ipAddress.GetAddressBytes();

            byte[] maskBytes = BitConverter.GetBytes(mask).Reverse().ToArray();

            byte[] startIPBytes = new byte[ipBytes.Length];
            byte[] endIPBytes = new byte[ipBytes.Length];

            for (int i = 0; i < ipBytes.Length; i++)
            {
                startIPBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
                endIPBytes[i] = (byte)(ipBytes[i] | ~maskBytes[i]);
            }

            // Convert the bytes to IP addresses.
            IPAddress startIP = new IPAddress(startIPBytes);
            IPAddress endIP = new IPAddress(endIPBytes);
            return new IPRange(startIP, endIP);
        }

        public static string ExtractFirstIpFromLine(string input)
        {
            string ipInSubLine = @"(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9][0-9]|[0-9])";
            Regex r = new Regex(ipInSubLine, RegexOptions.Singleline);
            string m = r.Match(input).Value;
            
            if (m == null | m == "")
            {
                return null;
            }
            else
            {
                return m;
            }
        }

        public static bool IsReservedIP(IPAddress ipAddress)
        {
            uint uintIp = IpToUint(ipAddress);

            //https://en.wikipedia.org/wiki/Reserved_IP_addresses
            string[] reservedSubnets = {
                                    "10.0.0.0/8",
                                    "100.64.0.0/10",
                                    "127.0.0.0/8",
                                    "169.254.0.0/16",
                                    "172.16.0.0/12",
                                    "192.0.0.0/24",
                                    "192.0.2.0/24",
                                    "192.88.99.0/24",
                                    "192.168.0.0/16",
                                    "198.18.0.0/15",
                                    "198.51.100.0/24",
                                    "203.0.113.0/24",
                                    "224.0.0.0/4",
                                    "240.0.0.0/4"
                                   };

            foreach (var subnet in reservedSubnets)
            {
                IPRange range = CalculateIpRange(subnet);
                uint uintStartIp = IpToUint(range.startIP);
                uint uintEndIp = IpToUint(range.endIP);
                if (uintIp >= uintStartIp & uintIp <= uintEndIp)
                {
                    return true;
                }
            }

            return false;
        }

        private class IPRange
        {
            public IPRange(IPAddress startIP, IPAddress endIP)
            {
                this.startIP = startIP;
                this.endIP = endIP;
            }
            public IPAddress startIP { get; set; }
            public IPAddress endIP { get; set; }
        }
    }
}
