using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace AquaDriveSP.Models
{
    [Table("resena", Schema = "AquaDriveSP")]
    public class Resena
    {
        public long ResenaId { get; set; }
        public long CitaId { get; set; }
        public long EmpleadoId { get; set; }
        public long ClienteId { get; set; }
        public short Puntuacion { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaCreacion { get; set; }

        public virtual Cita Cita { get; set; }
        public virtual Empleado Empleado { get; set; }
        public virtual Cliente Cliente { get; set; }
    }
}