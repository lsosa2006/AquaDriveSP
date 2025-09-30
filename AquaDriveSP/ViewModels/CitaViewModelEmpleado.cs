using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AquaDriveSP.ViewModels
{
    public class CitaViewModelEmpleado
    {
        public string Cliente { get; set; }
        public string Placa { get; set; }
        public string HoraInicio { get; set; }
        public string HoraFin { get; set; }
        public string Servicio { get; set; }
        public double Puntuacion { get; set; }
    }
}