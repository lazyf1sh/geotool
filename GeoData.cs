using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GeoTool
{
    public class GeoData : ICloneable
    {
        public IPAddress IpAddress { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Carrier { get; set; }
        public string Organisation { get; set; }
        public string CountryCode { get; set; }
        public string State { get; set; }
        public string Sld { get; set; }
        public string Type { get; set; }

        public object Clone()
        {
            return (GeoData) this.MemberwiseClone();
        }
    }
}