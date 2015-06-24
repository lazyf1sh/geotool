using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GeoTool
{
    public static class RamCache
    {
        private static Dictionary<IPAddress, GeoData> IPCache = new Dictionary<IPAddress, GeoData>();

        public static void Add(GeoData data)
        {
            GeoData geoDataToDict = (GeoData) data.Clone();

            /*
             for example:
             ipData.IPAddress: 255.101.18.34
             long longIp: 4284813858
             ip_address 24 mask is a IPCache key: 255.101.18.0
            */

            uint longIp = IpUtilities.IpToUint(geoDataToDict.IpAddress);
            IPAddress subnet24 = IpUtilities.UintToIp(longIp - (longIp % 256));
            geoDataToDict.IpAddress = subnet24;
            try
            {
                IPCache.Add(subnet24, geoDataToDict);
            }
            catch
            {
            }
        }

        public static GeoData Get(IPAddress ip)
        {
            uint longIp = IpUtilities.IpToUint(ip);
            IPAddress subnet24 = IpUtilities.UintToIp(longIp - (longIp % 256));

            try
            {
                GeoData geoData = IPCache[subnet24];
                geoData.IpAddress = ip;
                return geoData;
            }
            catch
            {
                return null;
            }
        }
    }
}
