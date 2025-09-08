using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace AquaDriveSP.Models
{
    [Table("cliente", Schema = "AquaDriveSP")]
    public class Cliente
    {
        public long clienteid { get; set; }
        public long usuarioid { get; set; }

        public virtual Usuario Usuario { get; set; }
        public virtual ICollection<Vehiculo> Vehiculos { get; set; }
        public virtual ICollection<Cita> Citas { get; set; }
        public virtual ICollection<Resena> Resenas { get; set; }

        public Cliente()
        {
            Vehiculos = new HashSet<Vehiculo>();
            Citas = new HashSet<Cita>();
            Resenas = new HashSet<Resena>();
        }
    }
}