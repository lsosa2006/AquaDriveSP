using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AquaDriveSP.ViewModels
{
    public class DashboardViewModel
    {
        public string Rol { get; set; }

        // Admin
        public DashboardAdminViewModel AdminData { get; set; }

        // Empleado
        public DashboardEmpleadoViewModel EmpleadoData { get; set; }

        // Cliente (puedes mejorar después)
        public List<string> ClienteCitas { get; set; }
    }
}