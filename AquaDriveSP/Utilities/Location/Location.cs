using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;

namespace AquaDriveSP.Utilities.Location
{
    public class Location
    {
        public double lat { get; set; }
        public double lng { get; set; }

        public Location ObtenerCoordenadas(string direccion)
        {
            string apiKey = "TU_API_KEY";
            string url = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(direccion)}&key={apiKey}";

            using (WebClient wc = new WebClient())
            {
                var json = wc.DownloadString(url);
                var js = new JavaScriptSerializer();
                dynamic data = js.Deserialize<dynamic>(json);
                if (data["status"] == "OK")
                {
                    double lat = data["results"][0]["geometry"]["location"]["lat"];
                    double lng = data["results"][0]["geometry"]["location"]["lng"];
                    return new Location { lat = lat, lng = lng };
                }
                else
                {
                    return null;
                }
            }
        }

        public double CalcularDistancia(double lat1, double lon1, double lat2, double lon2)
        {
            double R = 6371; // km
            double dLat = (lat2 - lat1) * Math.PI / 180;
            double dLon = (lon2 - lon1) * Math.PI / 180;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c; // distancia en km
        }
    }
}