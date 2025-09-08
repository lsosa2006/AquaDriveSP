using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace AquaDriveSP.Models
{
    [Table("cita", Schema = "AquaDriveSP")]
    public class Cita
    {
        public long CitaId { get; set; }
        public long ClienteId { get; set; }
        public long? EmpleadoId { get; set; }
        public long TipoServicioId { get; set; }
        public long SedeId { get; set; }
        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraFin { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public string Observaciones { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual Cliente Cliente { get; set; }
        public virtual Empleado Empleado { get; set; }
        public virtual TipoServicio TipoServicio { get; set; }
        public virtual Sede Sede { get; set; }
        public virtual Resena Resena { get; set; }
    }
}