using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AquaDriveSP.Models
{
    [Table("cita", Schema = "AquaDriveSP")]
    public class Cita
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long citaid { get; set; }

        public long clienteid { get; set; }
        public long? empleadoid { get; set; }
        public long tiposervicioid { get; set; }
        public long sedeid { get; set; }
        public string placa { get; set; }

        public DateTime fechahorainicio { get; set; }
        public DateTime? fechahorafin { get; set; }
        public int estado { get; set; } = 1;
        public string observaciones { get; set; }
        public DateTime createdat { get; set; }

        public virtual Cliente cliente { get; set; }
        public virtual Empleado empleado { get; set; }
        public virtual TipoServicio tiposervicio { get; set; }
        public virtual Sede sede { get; set; }
        public virtual Vehiculo vehiculo { get; set; }
        public virtual ICollection<Resena> resenas { get; set; }

        public Cita()
        {
            resenas = new HashSet<Resena>();
        }
    }
}
