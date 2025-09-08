using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AquaDriveSP.Models
{
    [Table("sede", Schema = "AquaDriveSP")]
    public class Sede
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long sedeid { get; set; }

        public string nombre { get; set; }
        public string direccion { get; set; }
        public string telefono { get; set; }

        public virtual ICollection<Empleado> empleados { get; set; }
        public virtual ICollection<Cita> citas { get; set; }

        public Sede()
        {
            empleados = new HashSet<Empleado>();
            citas = new HashSet<Cita>();
        }
    }
}
