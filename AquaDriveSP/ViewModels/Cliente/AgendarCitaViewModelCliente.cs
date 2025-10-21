using AquaDriveSP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AquaDriveSP.ViewModels
{
    public class AgendarCitaViewModelCliente
    {
        public List<Vehiculo> Vehiculos { get; set; }
        public List<Sede> Sedes { get; set; }
        public List<TipoServicio> TiposServicios { get; set; }
        public List<Empleado> Empleados { get; set; }
    }
}