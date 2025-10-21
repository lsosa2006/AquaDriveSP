using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AquaDriveSP.ViewModels
{
    public class DashboardClienteViewModel
    {
        public List<CitaClienteCard> UltimasCitas { get; set; } = new List<CitaClienteCard>();
        public CitaClienteCard ProximaCita { get; set; }
        public string SedeMasCercana { get; set; }
        public double LatitudSede { get; set; }
        public double LongitudSede { get; set; }
        public double DistanciaSedeKm { get; set; }
        public double LatitudCliente { get; set; }
        public double LongitudCliente { get; set; }

        public class CitaClienteCard
        {
            public string Sede { get; set; }
            public string Servicio { get; set; }
            public DateTime FechaHoraInicio { get; set; }
            public DateTime FechaHoraFin { get; set; }
            public string Vehiculo { get; set; }
        }

    }
}