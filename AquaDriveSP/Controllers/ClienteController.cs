using AquaDriveSP.Autentication;
using AquaDriveSP.DTO;
using AquaDriveSP.Models;
using AquaDriveSP.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AquaDriveSP.Controllers
{
    [AuthorizeRole("Cliente")]
    public class ClienteController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();
        #region Vistas
        public ActionResult AgendarCita()
        {
            try
            {
                long clienteId = Convert.ToInt64(Session["UsuarioId"]);

                var model = new AgendarCitaViewModelCliente
                {
                    Vehiculos = db.vehiculo
                        .Where(v => v.cliente.usuarioid == clienteId)
                        .ToList(),

                    Sedes = db.sede
                        .Where(s => s.sedeid != 0)
                        .ToList(),

                    TiposServicios = db.tiposervicio
                        .ToList(),

                    Empleados = db.empleado
                        .Where(e => e.estado == 1)
                        .ToList()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar los datos: " + ex.Message;
                return View(new AgendarCitaViewModelCliente());
            }
        }
        public ActionResult CitasPendientes()
        {
            return View();
        }
        #endregion

        #region Funciones
        // 1. Obtener empleados por sede
        // ---------------------------
        [HttpGet]
        public JsonResult GetEmpleadosPorSede(int sedeId)
        {
            try
            {
                var empleados = db.empleado
                    .Where(e => e.sedeid == sedeId && e.estado == 1)
                    .Select(e => new
                    {
                        empleadoid = e.empleadoid,
                        Nombre = e.usuario.nombre + " " + e.usuario.apellido
                    }).ToList();

                return Json(new { success = true, empleados }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // ---------------------------
        // 2. Validar horario de empleado
        // ---------------------------
        [HttpGet]
        public JsonResult ValidarHorario(int empleadoId, string fecha, string hora)
        {
            try
            {
                var empleado = db.empleado.Find(empleadoId);
                if (empleado == null)
                    return Json(new { success = false, message = "Empleado no encontrado." }, JsonRequestBehavior.AllowGet);

                DateTime fechaHora;
                if (!DateTime.TryParse($"{fecha} {hora}", out fechaHora))
                    return Json(new { success = false, message = "Fecha u hora inválida." }, JsonRequestBehavior.AllowGet);

                // Día de la semana (6 = domingo, 0 = lunes)
                int diaSemana = ((int)fechaHora.DayOfWeek + 6) % 7;

                // Obtener horario del empleado para ese día
                var horario = db.horario
                    .FirstOrDefault(h => h.empleadoid == empleadoId && h.diasemana == diaSemana && h.estado == 1);

                if (horario == null)
                    return Json(new { success = true, disponible = false, mensaje = "El empleado no trabaja ese día." }, JsonRequestBehavior.AllowGet);

                // Revisar si la hora está dentro del horario laboral
                TimeSpan horaInicio = horario.horainicio;
                TimeSpan horaFin = horario.horafin;
                TimeSpan horaSeleccionada = fechaHora.TimeOfDay;

                if (horaSeleccionada < horaInicio || horaSeleccionada >= horaFin)
                {
                    return Json(new { success = true, disponible = false, mensaje = "El empleado no trabaja a esa hora." }, JsonRequestBehavior.AllowGet);
                }

                // Validar que no tenga otra cita asignada en ese horario (30 min)
                var citaExistente = db.cita
                    .Where(c => c.empleadoid == empleadoId && c.fechahorainicio == fechaHora)
                    .FirstOrDefault();

                if (citaExistente != null)
                {
                    return Json(new { success = true, disponible = false, mensaje = "El empleado ya tiene una cita en ese horario." }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = true, disponible = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // ---------------------------
        // 3. Agregar nueva cita
        // ---------------------------
        [HttpPost]
        public JsonResult AgregarCita(Cita model)
        {
            try
            {
                // Validar campos obligatorios
                var usuarioId = (long)Session["UsuarioId"];
                var cliente = db.cliente.FirstOrDefault(c => c.usuario.usuarioid == usuarioId);
                model.clienteid = cliente.clienteid;
                if (model.clienteid <= 0 || model.sedeid <= 0 || model.tiposervicioid <= 0 || string.IsNullOrEmpty(model.placa))
                {
                    return Json(new { success = false, mensaje = "Faltan datos obligatorios para agendar la cita." });
                }

                // Validar fecha y hora
                DateTime fechaHoraInicio;
                if (!DateTime.TryParse(model.fechahorainicio.ToString(), out fechaHoraInicio))
                    return Json(new { success = false, mensaje = "Fecha u hora inválida." });

                // Validar si el empleado tiene cita en ese horario
                if (model.empleadoid.HasValue)
                {
                    var citaExistente = db.cita
                        .Where(c => c.empleadoid == model.empleadoid.Value && c.fechahorainicio == fechaHoraInicio)
                        .FirstOrDefault();

                    if (citaExistente != null)
                        return Json(new { success = false, mensaje = "El empleado ya tiene una cita en ese horario." });
                }

                // Crear cita
                var cita = new Cita
                {
                    clienteid = model.clienteid,
                    empleadoid = model.empleadoid,
                    tiposervicioid = model.tiposervicioid,
                    sedeid = model.sedeid,
                    placa = model.placa,
                    fechahorainicio = fechaHoraInicio,
                    fechahorafin = fechaHoraInicio.AddMinutes(30),
                    estado = 1,
                    observaciones = model.observaciones ?? "",
                    createdat = DateTime.Now
                };

                db.cita.Add(cita);
                db.SaveChanges();

                return Json(new { success = true, mensaje = "Cita agendada exitosamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult GetMisCitas(int[] estados)//Reutilizar funcion desde mis citas enviando estado 1 y 2 y de historial enviando estado 0 y 3
        {
            try
            {
                // Obtener el id del cliente desde sesión
                var usuarioId = Session["UsuarioId"];

                var cliente = db.cliente.FirstOrDefault(c => c.usuario.usuarioid == (long)usuarioId);
                if (cliente == null)
                    return Json(new { success = false, message = "Cliente no encontrado." }, JsonRequestBehavior.AllowGet);

                var citas = db.cita
                    .Where(c => c.clienteid == cliente.clienteid && estados.Contains(c.estado))
                    .Select(c => new
                    {
                        CitaId = c.citaid,
                        Placa = c.placa,
                        FechaHoraInicio = c.fechahorainicio,
                        Sede = c.sede.nombre,
                        TipoServicio = c.tiposervicio.nombre,
                        Empleado = c.empleadoid != null ? c.empleado.usuario.nombre + " " + c.empleado.usuario.apellido : "-",
                        Estado = c.estado == 1 ? "Pendiente" : (c.estado == 2 ? "En curso" : c.estado == 3 ? "Finalizada" : "Cancelada")
                    }).ToList();

                return Json(new { success = true, citas = citas.OrderBy(c => c.FechaHoraInicio).ToList() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Cancelar cita
        [HttpPost]
        public JsonResult CancelarCita(long citaId)
        {
            try
            {
                var cita = db.cita.Find(citaId);
                if (cita == null)
                    return Json(new { success = false, mensaje = "Cita no encontrada." });

                if (cita.estado != 1) // Solo pendientes
                    return Json(new { success = false, mensaje = "Solo se pueden cancelar citas pendientes." });

                var ahora = DateTime.Now;
                if (cita.fechahorainicio <= ahora.AddHours(1)) // Solo si falta >1 hora
                    return Json(new { success = false, mensaje = "Solo se pueden cancelar citas con al menos 1 hora de anticipación." });

                cita.estado = 3; // Cancelada
                db.SaveChanges();

                return Json(new { success = true, mensaje = "Cita cancelada correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = ex.Message });
            }
        }
        #endregion
    }
}