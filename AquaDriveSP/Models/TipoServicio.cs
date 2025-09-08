using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AquaDriveSP.Models
{
    [Table("tiposervicio", Schema = "AquaDriveSP")]
    public class TipoServicio
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long tiposervicioid { get; set; }

        public string nombre { get; set; }
        public int duracionminutos { get; set; }
        public decimal precio { get; set; }

        public virtual ICollection<Cita> citas { get; set; }

        public TipoServicio()
        {
            citas = new HashSet<Cita>();
        }
    }
}
