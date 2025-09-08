using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace AquaDriveSP.Models
{
    [Table("usuario", Schema = "AquaDriveSP")]
    public class Usuario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long usuarioid { get; set; }
        public string nombre { get; set; }
        public string apellido { get; set; }
        public string email { get; set; }
        public string telefono { get; set; }
        public string contrasena { get; set; }
        public DateTime fechacreacion { get; set; }

        public virtual Cliente Cliente { get; set; }
        public virtual Empleado Empleado { get; set; }
        public virtual Administrador Administrador { get; set; }
    }
}