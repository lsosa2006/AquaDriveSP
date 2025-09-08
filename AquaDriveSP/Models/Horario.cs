using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace AquaDriveSP.Models
{
    [Table("horario", Schema = "AquaDriveSP")]
    public class Horario
    {
        public long HorarioId { get; set; }
        public long EmpleadoId { get; set; }
        public short DiaSemana { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }

        public virtual Empleado Empleado { get; set; }
    }
}