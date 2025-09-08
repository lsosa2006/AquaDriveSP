using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace AquaDriveSP.Models
{
    [Table("empleado", Schema = "AquaDriveSP")]
    public class Empleado
    {
        public long EmpleadoId { get; set; }
        public long UsuarioId { get; set; }
        public long SedeId { get; set; }
        public DateTime? FechaContratacion { get; set; }

        public string Estado { get; set; } = "Pendiente";

        public virtual Usuario Usuario { get; set; }
        public virtual Sede Sede { get; set; }
        public virtual ICollection<Horario> Horarios { get; set; }
        public virtual ICollection<Cita> Citas { get; set; }
        public virtual ICollection<Resena> Resenas { get; set; }

        public Empleado()
        {
            Horarios = new HashSet<Horario>();
            Citas = new HashSet<Cita>();
            Resenas = new HashSet<Resena>();
        }
    }
}