using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace AquaDriveSP.Models
{
    [Table("administrador", Schema = "AquaDriveSP")]
    public class Administrador
    {
        public long AdministradorId { get; set; }
        public long UsuarioId { get; set; }

        public string Estado { get; set; } = "Pendiente";

        public virtual Usuario Usuario { get; set; }
    }
}