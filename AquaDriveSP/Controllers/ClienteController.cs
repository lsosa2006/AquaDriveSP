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

        public ActionResult GestionarVehiculos()
        {
            return View();
        }

        public ActionResult HistorialCitas()
        {
            try
            {
                var usuarioId = (long)Session["UsuarioId"];
                var cliente = db.cliente.FirstOrDefault(c => c.usuarioid == usuarioId);

                if (cliente == null)
                    return RedirectToAction("Login", "Auth");

                // Traemos las citas finalizadas (3) o canceladas (4)
                var citasDb = db.cita
                    .Where(c => c.clienteid == cliente.clienteid && (c.estado == 3 || c.estado == 4))
                    .ToList();

                var citas = citasDb.Select(c => new CitaViewModelCliente
                {
                    citaid = c.citaid,
                    placa = c.placa,
                    FechaInicio = c.fechahorainicio.ToString("dd/MM/yyyy HH:mm"),
                    FechaFin = c.fechahorafin.HasValue ? c.fechahorafin.Value.ToString("HH:mm") : "--",
                    Estado = c.estado == 3 ? "Finalizada" : "Cancelada",
                    Sede = c.sede != null ? c.sede.nombre : "N/A",
                    TipoServicio = c.tiposervicio != null ? c.tiposervicio.nombre : "N/A",
                    Empleado = c.empleado != null ? c.empleado.usuario.nombre + " " + c.empleado.usuario.apellido : "N/A",
                    TieneResena = c.resenas.Any(r => r.clienteid == cliente.clienteid)
                }).ToList();

                return View(citas);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al obtener el historial: " + ex.Message;
                return View();
            }
        }


        public ActionResult Resenas()
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
        //VEHICULOS
        [HttpGet]
        
        public JsonResult GetVehiculos()
        {
            try
            {
                var usuarioId = (long)Session["UsuarioId"];
                var cliente = db.cliente.FirstOrDefault(c => c.usuarioid == usuarioId);


                var vehiculos = db.vehiculo
                    .Where(v => v.clienteid == cliente.clienteid)
                    .Select(v => new
                    {
                        v.placa,
                        v.marca,
                        v.modelo,
                        v.color
                    }).ToList();

                return Json(new { success = true, data = vehiculos }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener los vehículos: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult CrearVehiculo(Vehiculo vehiculo)
        {
            try
            {
                var usuarioId = (long)Session["UsuarioId"];
                var cliente = db.cliente.FirstOrDefault(c => c.usuarioid == usuarioId);

                if (string.IsNullOrEmpty(vehiculo.placa))
                    return Json(new { success = false, message = "La placa es obligatoria" });

                // Validar placa duplicada
                bool placaExiste = db.vehiculo.Any(v => v.placa == vehiculo.placa);
                if (placaExiste)
                    return Json(new { success = false, message = "Ya existe un vehículo con esa placa" });

                vehiculo.clienteid = cliente.clienteid;

                db.vehiculo.Add(vehiculo);
                db.SaveChanges();

                return Json(new { success = true, message = "Vehículo agregado correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al agregar el vehículo: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult EditarVehiculo(Vehiculo vehiculo)
        {
            try
            {
                var usuarioId = (long)Session["UsuarioId"];
                var cliente = db.cliente.FirstOrDefault(c => c.usuarioid == usuarioId);
                

                var vehiculoExistente = db.vehiculo.FirstOrDefault(v => v.placa == vehiculo.placa && v.clienteid == cliente.clienteid);
                if (vehiculoExistente == null)
                    return Json(new { success = false, message = "Vehículo no encontrado o no pertenece al cliente" });

                vehiculoExistente.marca = vehiculo.marca;
                vehiculoExistente.modelo = vehiculo.modelo;
                vehiculoExistente.color = vehiculo.color;

                db.SaveChanges();
                return Json(new { success = true, message = "Vehículo actualizado correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al editar el vehículo: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult EliminarVehiculo(string placa)
        {
            try
            {
                var usuarioId = (long)Session["UsuarioId"];
                var cliente = db.cliente.FirstOrDefault(c => c.usuarioid == usuarioId);

                var vehiculo = db.vehiculo.FirstOrDefault(v => v.placa == placa && v.clienteid == cliente.clienteid);
                if (vehiculo == null)
                    return Json(new { success = false, message = "Vehículo no encontrado o no pertenece al cliente" });

                db.vehiculo.Remove(vehiculo);
                db.SaveChanges();

                return Json(new { success = true, message = "Vehículo eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar el vehículo: " + ex.Message });
            }
        }
        

        //RESEÑA
        [HttpPost]
        public JsonResult CrearResena(long citaid, short puntuacion, string descripcion)
        {
            try
            {
                var usuarioId = (long)Session["UsuarioId"];
                var cliente = db.cliente.FirstOrDefault(c => c.usuarioid == usuarioId);
                var cita = db.cita.FirstOrDefault(c => c.citaid == citaid && c.clienteid == cliente.clienteid);

                if (cita == null)
                    return Json(new { success = false, message = "Cita no encontrada o no pertenece al cliente." });

                if (cita.estado != 3)
                    return Json(new { success = false, message = "Solo se pueden calificar citas finalizadas." });

                bool yaExiste = db.resena.Any(r => r.citaid == citaid && r.clienteid == cliente.clienteid);
                if (yaExiste)
                    return Json(new { success = false, message = "Ya calificaste esta cita." });

                var resena = new Resena
                {
                    citaid = citaid,
                    empleadoid = cita.empleadoid ?? 0,
                    clienteid = cliente.clienteid,
                    puntuacion = puntuacion,
                    descripcion = descripcion,
                    fechacreacion = DateTime.Now
                };

                db.resena.Add(resena);
                db.SaveChanges();

                return Json(new { success = true, message = "Reseña enviada correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al guardar la reseña: " + ex.Message });
            }
        }

        //MODULO DE RESEÑAS
        [HttpGet]
        public JsonResult GetResenas()
        {
            try
            {
                var usuarioId = (long)Session["UsuarioId"];
                var cliente = db.cliente.FirstOrDefault(c => c.usuarioid == usuarioId);
                if (cliente == null)
                    return Json(new { success = false, message = "Cliente no encontrado." }, JsonRequestBehavior.AllowGet);

                // 1) Seleccionamos solo los campos que necesitamos (sin formatear fechas)
                var resenasQuery = db.resena
                    .Where(r => r.clienteid == cliente.clienteid)
                    .Select(r => new
                    {
                        r.resenaid,
                        r.puntuacion,
                        r.descripcion,
                        r.fechacreacion,
                        // navegación segura: puede devolver null si alguna relación falta
                        Servicio = r.cita.tiposervicio != null ? r.cita.tiposervicio.nombre : null,
                        EmpleadoNombre = r.cita.empleado != null && r.cita.empleado.usuario != null
                                         ? r.cita.empleado.usuario.nombre + " " + r.cita.empleado.usuario.apellido
                                         : null,
                        Placa = r.cita.vehiculo != null ? r.cita.vehiculo.placa : null
                    })
                    .OrderByDescending(r => r.resenaid)
                    .ToList(); // <-- aquí se ejecuta la consulta en la BD (sin ToString(format))

                // 2) Formateamos en memoria (aquí sí podemos usar ToString("dd/MM/yyyy"))
                var resenas = resenasQuery.Select(r => new
                {
                    r.resenaid,
                    Servicio = string.IsNullOrEmpty(r.Servicio) ? "N/A" : r.Servicio,
                    Empleado = string.IsNullOrEmpty(r.EmpleadoNombre) ? "N/A" : r.EmpleadoNombre,
                    Puntuacion = r.puntuacion,
                    Descripcion = r.descripcion,
                    Fecha = r.fechacreacion.ToString("dd/MM/yyyy"),
                    Placa = string.IsNullOrEmpty(r.Placa) ? "N/A" : r.Placa
                }).ToList();

                return Json(new { success = true, data = resenas }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener las reseñas: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public JsonResult EditarResena(long resenaid, short puntuacion, string descripcion)
        {
            try
            {
                var usuarioId = (long)Session["UsuarioId"];
                var cliente = db.cliente.FirstOrDefault(c => c.usuarioid == usuarioId);

                var resena = db.resena.FirstOrDefault(r => r.resenaid == resenaid && r.clienteid == cliente.clienteid);
                if (resena == null)
                    return Json(new { success = false, message = "Reseña no encontrada o no pertenece al cliente" });

                resena.puntuacion = puntuacion;
                resena.descripcion = descripcion;
                db.SaveChanges();

                return Json(new { success = true, message = "Reseña actualizada correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al editar la reseña: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult EliminarResena(long resenaid)
        {
            try
            {
                var usuarioId = (long)Session["UsuarioId"];
                var cliente = db.cliente.FirstOrDefault(c => c.usuarioid == usuarioId);

                var resena = db.resena.FirstOrDefault(r => r.resenaid == resenaid && r.clienteid == cliente.clienteid);
                if (resena == null)
                    return Json(new { success = false, message = "Reseña no encontrada o no pertenece al cliente" });

                db.resena.Remove(resena);
                db.SaveChanges();

                return Json(new { success = true, message = "Reseña eliminada correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar la reseña: " + ex.Message });
            }
        }
        #endregion
    }
}