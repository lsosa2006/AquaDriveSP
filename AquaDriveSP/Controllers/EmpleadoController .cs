using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Services.Description;
using AquaDriveSP.Autentication;
using AquaDriveSP.Models;
using AquaDriveSP.ViewModels;

namespace AquaDriveSP.Controllers
{
    [AuthorizeRole("Empleado")]
    public class EmpleadoController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();
        #region Vistas
        public ActionResult GestionarCitas()
        {
            return View();
        }
        public ActionResult HistorialCitas()
        {
            try
            {
                var empleadoId = (long)Session["UsuarioId"];

                // Traemos las citas a memoria primero
                var citasFinalizadasDb = db.cita
                    .Where(c => c.empleado.usuario.usuarioid == empleadoId && c.estado == 3)
                    .ToList(); // <-- traemos los datos a memoria

                // Formateamos fechas y calculamos promedios en memoria
                var citasFinalizadas = citasFinalizadasDb
                    .Select(c => new CitaViewModelEmpleado
                    {
                        Cliente = c.cliente.usuario.nombre,
                        Placa = c.vehiculo != null ? c.vehiculo.placa : "N/A",
                        HoraInicio = c.fechahorainicio.ToString("dd/MM/yyyy HH:mm"),
                        HoraFin = c.fechahorafin.HasValue ? c.fechahorafin.Value.ToString("HH:mm") : "--",
                        Servicio = c.tiposervicio != null ? c.tiposervicio.nombre : "N/A",
                        Puntuacion = c.resenas.Any() ? c.resenas.Average(r => r.puntuacion) : 0
                    }).ToList();

                return View(citasFinalizadas);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al obtener citas: " + ex.Message;
                return View();
            }
        }

        public ActionResult Horario()
        {
            try
            {
                var empleadoId = (long)Session["UsuarioId"];
                var horarios = db.horario
                    .Where(h => h.empleado.usuarioid == empleadoId)
                    .ToList();

                return View(horarios);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al obtener horario: " + ex.Message;
                return View();
            }
        }
        #endregion

        #region Funciones
        // Obtener citas asignadas del empleado (día actual)
        [HttpGet]
        public JsonResult GetCitasAsignadas()
        {
            try
            {
                var empleadoId = (long)Session["UsuarioId"];
                var inicioDia = DateTime.Today;
                var finDia = inicioDia.AddDays(1);

                var citasDb = db.cita
                    .Where(c => c.empleado.usuario.usuarioid == empleadoId &&
                                c.fechahorainicio >= inicioDia &&
                                c.fechahorainicio < finDia &&
                                c.estado != 3 && c.estado != 0)
                    .ToList();

                var citas = citasDb.Select(c => new
                {
                    c.citaid,
                    cliente = c.cliente.usuario.nombre,
                    vehiculo = c.vehiculo != null ? c.vehiculo.placa : "N/A",
                    servicio = c.tiposervicio != null ? c.tiposervicio.nombre : "N/A",
                    sede = c.sede != null ? c.sede.nombre : "N/A",
                    horainicio = c.fechahorainicio.ToString("HH:mm"),
                    horafin = c.fechahorafin.HasValue ? c.fechahorafin.Value.ToString("HH:mm") : "--",
                    estado = c.estado
                }).ToList();

                return Json(new { success = true, citas }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener citas: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        //Obtener una sola cita
        [HttpGet]
        public JsonResult GetDetalleCita(long citaId)
        {
            try
            {
                var citaDb = db.cita
                    .Where(c => c.citaid == citaId)
                    .FirstOrDefault();

                if (citaDb == null)
                    return Json(new { success = false, message = "Cita no encontrada" }, JsonRequestBehavior.AllowGet);

                var cita = new
                {
                    citaDb.citaid,
                    cliente = citaDb.cliente.usuario.nombre,
                    vehiculo = citaDb.vehiculo != null ? citaDb.vehiculo.placa : "N/A",
                    servicio = citaDb.tiposervicio != null ? citaDb.tiposervicio.nombre : "N/A",
                    sede = citaDb.sede != null ? citaDb.sede.nombre : "N/A",
                    horainicio = citaDb.fechahorainicio.ToString("HH:mm"),
                    horafin = citaDb.fechahorafin.HasValue ? citaDb.fechahorafin.Value.ToString("HH:mm") : "--",
                    estado = citaDb.estado
                };

                return Json(new { success = true, cita }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener cita: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Cambiar estado de una cita
        [HttpPost]
        public JsonResult CambiarEstadoCita(long citaId, int estado)
        {
            try
            {
                var cita = db.cita.FirstOrDefault(c => c.citaid == citaId);
                if (cita == null)
                    return Json(new { success = false, message = "Cita no encontrada." });

                cita.estado = estado;
                string message = "";

                if (estado == 2)
                {
                    // En curso
                    cita.fechahorainicio = DateTime.Now;
                    message = "Cita iniciada";
                }

                if (estado == 3) 
                {
                    // Finalizada
                    cita.fechahorafin = DateTime.Now;
                    message = "Cita finalizada";
                }

                if (estado == 0)
                {
                    message = "Cita cancelada";
                }

                db.SaveChanges();
                return Json(new { success = true, message = message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al cambiar estado: " + ex.Message });
            }
        }
        #endregion
    }
}