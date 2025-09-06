using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AquaDriveSP.Models
{
    public class Sede
    {
        public long SedeId { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }

        public virtual ICollection<Empleado> Empleados { get; set; }
        public virtual ICollection<Cita> Citas { get; set; }

        public Sede()
        {
            Empleados = new HashSet<Empleado>();
            Citas = new HashSet<Cita>();
        }
    }
}