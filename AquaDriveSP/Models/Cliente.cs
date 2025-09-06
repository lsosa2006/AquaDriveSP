using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AquaDriveSP.Models
{
    public class Cliente
    {
        public long ClienteId { get; set; }
        public long UsuarioId { get; set; }

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