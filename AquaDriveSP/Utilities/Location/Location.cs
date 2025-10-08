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
            // URL de Nominatim (OpenStreetMap)
            string url = $"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(direccion)}";

            using (WebClient wc = new WebClient())
            {
                // Es obligatorio incluir un User-Agent para evitar bloqueos del servidor
                wc.Headers.Add("User-Agent", "AquaDriveSP/1.0 (contacto@tucorreo.com)");

                var json = wc.DownloadString(url);
                var js = new JavaScriptSerializer();
                var data = js.Deserialize<List<dynamic>>(json);

                if (data != null && data.Count > 0)
                {
                    double lat = Convert.ToDouble(data[0]["lat"], System.Globalization.CultureInfo.InvariantCulture);
                    double lng = Convert.ToDouble(data[0]["lon"], System.Globalization.CultureInfo.InvariantCulture);
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