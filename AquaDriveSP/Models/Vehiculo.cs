using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AquaDriveSP.Models
{
    [Table("vehiculo", Schema = "AquaDriveSP")]
    public class Vehiculo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Placa no es autoincremental
        public string Placa { get; set; }
        public long ClienteId { get; set; }
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public string Color { get; set; }

        public virtual Cliente Cliente { get; set; }
    }
}