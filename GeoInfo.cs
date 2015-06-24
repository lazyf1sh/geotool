using QuovaApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GeoTool
{
    public class GeoInfo
    {
        private IPAddress _ip;
        private GeoData geodata;
        public GeoInfo(IPAddress ip)
        {
            this._ip = ip;
        }

        public GeoData Get()
        {
            if (IpUtilities.IsReservedIP(_ip))
            {
                return null;
            }

            geodata = RamCache.Get(_ip);
            if (geodata != null)
            {
                return geodata;
            }

            geodata = SQLiteCache.Get(_ip);
            if (geodata != null)
            {
                RamCache.Add(geodata);
                return geodata;
            }

            string api = File.ReadAllLines("api+secret")[0].Split('|')[0];
            string secret = File.ReadAllLines("api+secret")[0].Split('|')[1];
            Quova q = new Quova(api, secret);
            IpInfo ipInfo = q.LookUp(_ip);
            if (ipInfo != null)
            {
                geodata = new GeoData()
                {
                    IpAddress = _ip,
                    Country = ipInfo.Location.CountryData.country,
                    City = ipInfo.Location.CityData.city,
                    Organisation = ipInfo.Network.organization,
                    Carrier = ipInfo.Network.carrier,
                    CountryCode = ipInfo.Location.CountryData.country_code,
                    State = ipInfo.Location.StateData.state,
                    Sld = ipInfo.Network.Domain.sld
                };
                RamCache.Add(geodata);
                SQLiteCache.Add(geodata);
                return geodata;
            }

            return null;
        }
    }
}
