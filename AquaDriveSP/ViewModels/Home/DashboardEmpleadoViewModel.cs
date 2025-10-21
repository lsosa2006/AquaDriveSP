using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AquaDriveSP.ViewModels
{
    public class DashboardEmpleadoViewModel
    {
        // 3 citas más recientes asignadas del día
        public List<CitaCard> CitasAsignadasHoy { get; set; }

        // Citas finalizadas hoy
        public List<CitaCard> CitasFinalizadasHoy { get; set; }

        // Historial últimos 7 días
        public List<string> DiasUltimos7Dias { get; set; }
        public List<int> CitasUltimos7Dias { get; set; }

        public class CitaCard
        {
            public string Cliente { get; set; }
            public string Vehiculo { get; set; }
            public string Servicio { get; set; }
            public DateTime HoraInicio { get; set; }
            public DateTime? HoraFin { get; set; }
        }
    }
}
