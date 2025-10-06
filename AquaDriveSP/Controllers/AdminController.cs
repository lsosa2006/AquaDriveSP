using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Services.Description;
using AquaDriveSP.Autentication;
using AquaDriveSP.Models;
using AquaDriveSP.DTO;

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
        public ActionResult GestionarSede()
        {
            return View();
        }
        public ActionResult GestionarServicio()
        {
            return View();
        }
        #endregion

        #region Funciones
        // Obtener empleados con usuario y sede
        [HttpGet]
        public JsonResult GetEmpleados()
        {
            try
            {
                // Traer los empleados con usuario y sede 
                var empleadosDb = db.empleado
                .Include("usuario")
                .Include("sede")
                .ToList();

                // Mapear los datos a un objeto listo para JSON
                var empleados = empleadosDb.Select(e => new
                {
                    e.empleadoid,
                    e.usuarioid,
                    e.sedeid,
                    fechacontratacion = e.fechacontratacion.HasValue
                        ? e.fechacontratacion.Value.ToString("yyyy-MM-dd")
                        : null,
                    e.estado,
                    usuario = new
                    {
                        e.usuario.nombre,
                        e.usuario.apellido,
                        e.usuario.email,
                        e.usuario.telefono,
                        e.usuario.contrasena
                    },
                    sede = e.sede != null ? e.sede.nombre : null
                }).ToList();

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

        // Actualizar varios empleados (sede, fecha, estado)
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

        //Eliminar empleado
        [HttpPost]
        public JsonResult DeleteEmpleado(long id)
        {
            try
            {
                var emp = db.empleado.FirstOrDefault(e => e.empleadoid == id);
                if (emp == null)
                    return Json(new { success = false, message = "Empleado no encontrado." });

                var usuario = db.usuario.FirstOrDefault(u => u.usuarioid == emp.usuarioid);

                db.empleado.Remove(emp);
                if (usuario != null)
                    db.usuario.Remove(usuario);

                db.SaveChanges();
                return Json(new { success = true, message = "Empleado y usuario eliminados." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al eliminar empleado: {ex.Message}" });
            }
        }

        // Obtener horario de un empleado
        [HttpGet]
        public JsonResult GetHorario(long empleadoId)
        {
            try
            {
                var horariosDb = db.horario
                    .Where(h => h.empleadoid == empleadoId)
                    .ToList();

                var dias = Enumerable.Range(0, 7).ToList();

                var horarios = dias.Select(dia =>
                {
                    var h = horariosDb.FirstOrDefault(x => x.diasemana == dia);
                    return new
                    {
                        horarioid = h?.horarioid ?? 0,
                        diasemana = dia,
                        horainicio = h != null ? h.horainicio.ToString(@"hh\:mm") : "",
                        horafin = h != null ? h.horafin.ToString(@"hh\:mm") : "",
                        estado = h?.estado ?? 0
                    };
                }).ToList();

                return Json(new { success = true, message = "Horario obtenido con éxito", horarios },
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al obtener horario: {ex.Message}" },
                    JsonRequestBehavior.AllowGet);
            }
        }

        // Guardar/actualizar horario de un solo empleado
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
                        .FirstOrDefault(x => x.empleadoid == empleadoId && x.diasemana == h.diasemana);

                    if (horarioDb != null)
                    {
                         horarioDb.horainicio = h.horainicio;
                         horarioDb.horafin = h.horafin;
                         horarioDb.estado = h.estado;
                    }
                    else
                    {
                        db.horario.Add(new Horario
                        {
                            empleadoid = empleadoId,
                            diasemana = h.diasemana,
                            horainicio = h.horainicio,
                            horafin = h.horafin,
                            estado = h.estado
                        });
                    }
                }

                db.SaveChanges();
                return Json(new { success = true, message = "Horario guardado correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al guardar horarios: {ex.Message}" });
            }
        }
        // Guardar/actualizar horario de todos los empleados empleado
        [HttpPost]
        public JsonResult SaveHorarios(List<EmpleadoHorarioDto> empleadosHorarios)
        {
            try
            {
                if (empleadosHorarios == null || empleadosHorarios.Count == 0)
                    return Json(new { success = false, message = "No se enviaron datos de empleados." });

                foreach (var emp in empleadosHorarios)
                {
                    if (emp.EmpleadoId <= 0 || emp.Horarios == null)
                        continue;

                    foreach (var h in emp.Horarios)
                    {
                        var horarioDb = db.horario
                            .FirstOrDefault(x => x.empleadoid == emp.EmpleadoId && x.diasemana == h.diasemana);

                        if (horarioDb != null)
                        {
                            // Actualizar
                            horarioDb.horainicio = h.horainicio;
                            horarioDb.horafin = h.horafin;
                            horarioDb.estado = h.estado;
                        }
                        else
                        {
                            // Insertar
                            db.horario.Add(new Horario
                            {
                                empleadoid = emp.EmpleadoId,
                                diasemana = h.diasemana,
                                horainicio = h.horainicio,
                                horafin = h.horafin,
                                estado = h.estado
                            });
                        }
                    }
                }

                db.SaveChanges();
                return Json(new { success = true, message = "Horarios guardados correctamente para todos los empleados." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al guardar horarios: {ex.Message}" });
            }
        }

        // Obtener todos los administradores
        [HttpGet]
        public JsonResult GetAdministradores()
        {
            try
            {
                var admins = db.administrador
                    .Include("usuario")
                    .ToList()
                    .Select(a => new
                    {
                        a.administradorid,
                        a.usuarioid,
                        usuario = new
                        {
                            a.usuario.nombre,
                            a.usuario.apellido,
                            a.usuario.email,
                            a.usuario.telefono,
                            a.usuario.contrasena
                        }
                    }).ToList();

                return Json(new { success = true, message = "Administradores obtenidos con éxito", administradores = admins }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al obtener administradores: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        // Eliminar un administrador
        [HttpPost]
        public JsonResult DeleteAdministrador(long adminid)
        {
            try
            {
                var admin = db.administrador.FirstOrDefault(a => a.administradorid == adminid);
                if (admin == null)
                    return Json(new { success = false, message = "Administrador no encontrado." });

                var usuario = db.usuario.FirstOrDefault(u => u.usuarioid == admin.usuarioid);

                db.administrador.Remove(admin);
                if (usuario != null)
                    db.usuario.Remove(usuario);

                db.SaveChanges();

                return Json(new { success = true, message = "Administrador eliminado correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al eliminar administrador: {ex.Message}" });
            }
        }

        //Obtener estadisticas
        public JsonResult GetEstadisticas(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                // Autos lavados por empleado
                var autosPorEmpleado = db.empleado
                    .Select(emp => new
                    {
                        Nombre = emp.usuario.nombre + " " + emp.usuario.apellido,
                        Cantidad = emp.citas
                            .Count(c => c.estado == 3 &&
                                        c.fechahorafin >= fechaInicio &&
                                        c.fechahorafin <= fechaFin)
                    })
                    .Where(x => x.Cantidad > 0)
                    .ToList();

                // Servicios por sede
                var serviciosPorSede = db.sede
                    .Where(s => s.sedeid != 0)
                    .Select(s => new
                    {
                        s.nombre,
                        Cantidad = s.citas
                            .Count(c => c.estado == 3 &&
                                        c.fechahorafin >= fechaInicio &&
                                        c.fechahorafin <= fechaFin)
                    })
                    .Where(x => x.Cantidad > 0)
                    .ToList();

                // Total por tipo de servicio
                var ingresosPorServicio = db.tiposervicio
                    .Select(ts => new
                    {
                        ts.nombre,
                        Precio = ts.precio,
                        Cantidad = ts.citas
                            .Count(c => c.estado == 3 &&
                                        c.fechahorafin >= fechaInicio &&
                                        c.fechahorafin <= fechaFin),
                        Total = ts.citas
                            .Where(c => c.estado == 3 &&
                                        c.fechahorafin >= fechaInicio &&
                                        c.fechahorafin <= fechaFin)
                            .Select(c => (decimal?)c.tiposervicio.precio)
                            .DefaultIfEmpty(0)
                            .Sum() ?? 0
                    })
                    .Where(x => x.Cantidad > 0)
                    .ToList();

                return Json(new
                {
                    success = true,
                    AutosPorEmpleado = autosPorEmpleado,
                    ServiciosPorSede = serviciosPorSede,
                    IngresosPorServicio = ingresosPorServicio
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error al obtener estadísticas: " + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // Obtener sedes
        [HttpGet]
        public JsonResult GetSedes()
        {
            try
            {
                var sedes = db.sede
                    .Select(s => new
                    {
                        s.sedeid,
                        s.nombre,
                        s.direccion,
                        s.telefono
                    }).ToList();

                return Json(new { success = true, message = "Sedes obtenidas con éxito", sedes }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al obtener sedes: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        //Crear sedes
        [HttpPost]
        public JsonResult CrearSede(Sede nuevaSede)
        {
            try
            {
                var geo = new Utilities.Location.Location();
                var coordenadas = geo.ObtenerCoordenadas(nuevaSede.direccion);

                nuevaSede.latitud = coordenadas.lat;
                nuevaSede.longitud = coordenadas.lng;
                db.sede.Add(nuevaSede);
                db.SaveChanges();

                return Json(new { success = true, message = "Sede creada con éxito" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al crear sede: {ex.Message}" });
            }
        }

        //Eliminar sede
        [HttpPost]
        public JsonResult DeleteSede(long sedeid)
        {
            try
            {
                var sede = db.sede.Find(sedeid);
                if (sede == null)
                    return Json(new { success = false, message = "Sede no encontrada" });

                db.sede.Remove(sede);
                db.SaveChanges();

                return Json(new { success = true, message = "Sede eliminada con éxito" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al eliminar sede: {ex.Message}" });
            }
        }

        //Actualizar sede
        [HttpPost]
        public JsonResult EditarSede(Sede sedeActualizada)
        {
            try
            {
                var sede = db.sede.Find(sedeActualizada.sedeid);
                if (sede == null)
                    return Json(new { success = false, message = "Sede no encontrada" });

                sede.nombre = sedeActualizada.nombre;
                sede.direccion = sedeActualizada.direccion;
                sede.telefono = sedeActualizada.telefono;

                db.SaveChanges();
                return Json(new { success = true, message = "Sede actualizada con éxito" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al editar sede: {ex.Message}" });
            }
        }


        //Obtener servicios
        [HttpGet]
        public JsonResult GetServicios()
        {
            try
            {
                var servicio = db.tiposervicio
                    .Select(ts => new
                    {
                        ts.tiposervicioid,
                        ts.nombre,
                        ts.duracionminutos,
                        ts.precio
                    }).ToList();

                return Json(new { success = true, message = "Servicios obtenido con éxito", servicio }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al obtener los servicios: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        //Crear servicios
        [HttpPost]
        public JsonResult CrearServicio(TipoServicio nuevoServicio)
        {
            try
            {
                db.tiposervicio.Add(nuevoServicio);
                db.SaveChanges();

                return Json(new { success = true, message = "Servicio creado con éxito" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al crear servicio: {ex.Message}" });
            }
        }

        //Eliminar servicios
        [HttpPost]
        public JsonResult DeleteServicio(long servicioid)
        {
            try
            {
                var servicio = db.tiposervicio.Find(servicioid);
                if (servicio == null)
                    return Json(new { success = false, message = "Servicio no encontrado" });

                db.tiposervicio.Remove(servicio);
                db.SaveChanges();

                return Json(new { success = true, message = "Servicio eliminado con éxito" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al eliminar Servicio: {ex.Message}" });
            }
        }

        //Editar servicios
        [HttpPost]
        public JsonResult EditarServicio(TipoServicio servicioActualizado)
        {
            try
            {
                var servicio = db.tiposervicio.Find(servicioActualizado.tiposervicioid);
                if (servicio == null)
                    return Json(new { success = false, message = "Servicio no encontrado" });

                servicio.nombre = servicioActualizado.nombre;
                servicio.duracionminutos = servicioActualizado.duracionminutos;
                servicio.precio = servicioActualizado.precio;

                db.SaveChanges();

                return Json(new { success = true, message = "Servicio actualizado con éxito" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al editar servicio: {ex.Message}" });
            }
        }
        #endregion
    }
}