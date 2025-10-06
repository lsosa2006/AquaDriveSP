using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace AquaDriveSP.Models
{
    [Table("cliente", Schema = "AquaDriveSP")]
    public class Cliente
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long clienteid { get; set; }
        public long usuarioid { get; set; }
        public double latitud { get; set; }
        public double longitud { get; set; }

        public virtual Usuario usuario { get; set; }
        public virtual ICollection<Vehiculo> vehiculos { get; set; }
        public virtual ICollection<Cita> citas { get; set; }
        public virtual ICollection<Resena> resenas { get; set; }

        public Cliente()
        {
            vehiculos = new HashSet<Vehiculo>();
            citas = new HashSet<Cita>();
            resenas = new HashSet<Resena>();
        }
    }
}