using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AquaDriveSP.Models
{
    public class Usuario
    {
        public long UsuarioId { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Contrasena { get; set; }
        public DateTime FechaCreacion { get; set; }

        public virtual Cliente Cliente { get; set; }
        public virtual Empleado Empleado { get; set; }
        public virtual Administrador Administrador { get; set; }
    }
}