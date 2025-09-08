using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AquaDriveSP.Models
{
    [Table("horario", Schema = "AquaDriveSP")]
    public class Horario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long horarioid { get; set; }

        public long empleadoid { get; set; }
        public short diasemana { get; set; }
        public TimeSpan horainicio { get; set; }
        public TimeSpan horafin { get; set; }

        public virtual Empleado empleado { get; set; }
    }
}
