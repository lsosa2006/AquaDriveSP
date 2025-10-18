using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AquaDriveSP.ViewModels
{
    public class CitaViewModelCliente
    {
        public long citaid { get; set; }
        public string placa { get; set; }
        public string FechaInicio { get; set; }
        public string FechaFin { get; set; }
        public string Estado { get; set; }
        public string Sede { get; set; }
        public string TipoServicio { get; set; }
        public string Empleado { get; set; }
        public bool TieneResena { get; set; }
    }

}