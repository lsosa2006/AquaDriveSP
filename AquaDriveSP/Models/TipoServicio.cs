using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace AquaDriveSP.Models
{
    [Table("tiposervicio", Schema = "AquaDriveSP")]
    public class TipoServicio
    {
        public long TipoServicioId { get; set; }
        public string Nombre { get; set; }
        public int DuracionMinutos { get; set; }
        public decimal Precio { get; set; }

        public virtual ICollection<Cita> Citas { get; set; }

        public TipoServicio()
        {
            Citas = new HashSet<Cita>();
        }
    }
}