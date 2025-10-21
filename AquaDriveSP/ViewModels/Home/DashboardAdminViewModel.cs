using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AquaDriveSP.ViewModels
{
    public class DashboardAdminViewModel
    {
        // Top 3 empleados mejor calificados
        public List<string> TopEmpleados { get; set; }

        // Autos lavados por sede
        public Dictionary<string, int> AutosPorSede { get; set; }

        // Ingresos del día actual
        public decimal IngresosHoy { get; set; }

        // Ingresos últimos 7 días (Lunes, Martes...)
        public List<decimal> IngresosUltimos7Dias { get; set; }

        // Fechas de los últimos 7 días
        public List<string> DiasUltimos7Dias { get; set; }
    }
}