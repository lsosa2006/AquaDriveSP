using AquaDriveSP.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

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
        public string direccion { get; set; }
        public DateTime fechacreacion { get; set; }

        public virtual ICollection<Cliente> clientes { get; set; }
        public virtual ICollection<Empleado> empleados { get; set; }
        public virtual ICollection<Administrador> administradores { get; set; }
    }
}


