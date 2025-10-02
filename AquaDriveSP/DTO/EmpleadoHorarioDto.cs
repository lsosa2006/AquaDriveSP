using AquaDriveSP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AquaDriveSP.DTO
{
    public class EmpleadoHorarioDto
    {
        public long EmpleadoId { get; set; }
        public List<Horario> Horarios { get; set; }
    }
}