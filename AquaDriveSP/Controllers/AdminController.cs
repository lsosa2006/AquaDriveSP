using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Services.Description;
using AquaDriveSP.Autentication;
using AquaDriveSP.Models;

namespace AquaDriveSP.Controllers
{
    [AuthorizeRole("Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();
        #region Vistas
        public ActionResult GestionarAdministrador()
        {
            return View();
        }
        public ActionResult GestionarEmpleado()
        {
            return View();
        }
        public ActionResult Estadisticas()
        {
            return View();
        }
        #endregion

        #region Funciones
        // 1. Obtener empleados con usuario y sede
        [HttpGet]
        public JsonResult GetEmpleados()
        {
            try
            {
                var empleados = db.empleado
                    .Select(e => new
                    {
                        e.empleadoid,
                        e.usuarioid,
                        e.sedeid,
                        e.fechacontratacion,
                        e.estado,
                        usuario = new
                        {
                            e.usuario.nombre,
                            e.usuario.apellido,
                            e.usuario.email,
                            e.usuario.telefono
                        },
                        sede = e.sede != null ? e.sede.nombre : null
                    })
                    .ToList();

                return Json(new
                {
                    success = true,
                    message = "Empleados obtenidos con éxito",
                    empleados
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al obtener empleados: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        // 2. Actualizar varios empleados (sede, fecha, estado)
        [HttpPost]
        public JsonResult BulkUpdateEmpleados(List<Empleado> empleadosActualizados)
        {
            try
            {
                if (empleadosActualizados == null || empleadosActualizados.Count == 0)
                    return Json(new { success = false, message = "No se recibieron empleados." });

                foreach (var emp in empleadosActualizados)
                {
                    var empDb = db.empleado.FirstOrDefault(e => e.empleadoid == emp.empleadoid);
                    if (empDb != null)
                    {
                        empDb.sedeid = emp.sedeid;
                        empDb.fechacontratacion = emp.fechacontratacion;
                        empDb.estado = emp.estado;
                    }
                }

                db.SaveChanges();
                return Json(new { success = true, message = "Cambios guardados correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al actualizar empleados: {ex.Message}" });
            }
        }

        // 3. Eliminar empleado
        [HttpPost]
        public JsonResult DeleteEmpleado(long id)
        {
            try
            {
                var emp = db.empleado.FirstOrDefault(e => e.empleadoid == id);
                if (emp == null)
                    return Json(new { success = false, message = "Empleado no encontrado." });

                db.empleado.Remove(emp);
                db.SaveChanges();
                return Json(new { success = true, message = "Empleado eliminado." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al eliminar empleado: {ex.Message}" });
            }
        }

        // 4. Obtener sedes
        [HttpGet]
        public JsonResult GetSedes()
        {
            try
            {
                var sedes = db.sede
                    .Select(s => new { s.sedeid, s.nombre })
                    .ToList();

                return Json(new
                {
                    success = true,
                    message = "Sedes obtenidas con éxito",
                    sedes
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al obtener sedes: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        // 5. Obtener horario de un empleado
        [HttpGet]
        public JsonResult GetHorario(long empleadoId)
        {
            try
            {
                var horarios = db.horario
                    .Where(h => h.empleadoid == empleadoId)
                    .Select(h => new
                    {
                        h.horarioid,
                        h.diasemana,
                        horainicio = h.horainicio.ToString(@"hh\:mm"),
                        horafin = h.horafin.ToString(@"hh\:mm")
                    })
                    .ToList();

                return Json(new
                {
                    success = true,
                    message = "Horario obtenido con éxito",
                    horarios
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al obtener horario: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        // 6. Guardar/actualizar horarios
        [HttpPost]
        public JsonResult SaveHorario(long empleadoId, List<Horario> horarios)
        {
            try
            {
                if (empleadoId <= 0 || horarios == null)
                    return Json(new { success = false, message = "Datos inválidos" });

                foreach (var h in horarios)
                {
                    var horarioDb = db.horario
                        .FirstOrDefault(x => x.empleadoid == empleadoId &&
                                             x.diasemana == h.diasemana);

                    if (horarioDb != null)
                    {
                        // Actualizar
                        horarioDb.horainicio = h.horainicio;
                        horarioDb.horafin = h.horafin;
                    }
                    else
                    {
                        // Crear
                        var nuevo = new Horario
                        {
                            empleadoid = empleadoId,
                            diasemana = h.diasemana,
                            horainicio = h.horainicio,
                            horafin = h.horafin
                        };
                        db.horario.Add(nuevo);
                    }
                }

                db.SaveChanges();
                return Json(new { success = true, message = "Horarios guardados correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al guardar horarios: {ex.Message}" });
            }
        }
        #endregion

    }
}