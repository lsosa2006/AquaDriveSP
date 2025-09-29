using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AquaDriveSP.Models
{
    [Table("empleado", Schema = "AquaDriveSP")]
    public class Empleado
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long empleadoid { get; set; }

        public long usuarioid { get; set; }
        public long sedeid { get; set; }
        public DateTime? fechacontratacion { get; set; }
        public int estado { get; set; } = 0;

        public virtual Usuario usuario { get; set; }
        public virtual Sede sede { get; set; }

        public virtual ICollection<Horario> horarios { get; set; }
        public virtual ICollection<Cita> citas { get; set; }
        public virtual ICollection<Resena> resenas { get; set; }

        public Empleado()
        {
            horarios = new HashSet<Horario>();
            citas = new HashSet<Cita>();
            resenas = new HashSet<Resena>();
        }
    }
}
