using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AquaDriveSP.Models
{
    [Table("resena", Schema = "AquaDriveSP")]
    public class Resena
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long resenaid { get; set; }

        public long citaid { get; set; }
        public long empleadoid { get; set; }
        public long clienteid { get; set; }

        public short puntuacion { get; set; }
        public string descripcion { get; set; }
        public DateTime fechacreacion { get; set; }

        public virtual Cita cita { get; set; }
        public virtual Empleado empleado { get; set; }
        public virtual Cliente cliente { get; set; }
    }
}
