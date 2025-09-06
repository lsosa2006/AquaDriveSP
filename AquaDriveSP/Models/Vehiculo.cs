using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AquaDriveSP.Models
{
    public class Vehiculo
    {
        public string Placa { get; set; }
        public long ClienteId { get; set; }
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public string Color { get; set; }

        public virtual Cliente Cliente { get; set; }
    }
}