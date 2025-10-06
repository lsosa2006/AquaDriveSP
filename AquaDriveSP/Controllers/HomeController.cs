using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AquaDriveSP.Autentication;
using AquaDriveSP.Models;
using AquaDriveSP.ViewModels;
using static AquaDriveSP.ViewModels.DashboardClienteViewModel;
using static AquaDriveSP.ViewModels.DashboardEmpleadoViewModel;


namespace AquaDriveSP.Controllers
{
    [AuthorizeRole("Admin", "Empleado", "Cliente")]
    public class HomeController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();


        public ActionResult Dashboard()
        {
            string rol = Session["Rol"]?.ToString() ?? "Invitado";
            var vm = new DashboardViewModel
            {
                Rol = rol
            };

            if (rol == "Admin")
            {
                var adminVm = new DashboardAdminViewModel();
                var hoyInicio = DateTime.Today;
                var hoyFin = hoyInicio.AddDays(1).AddTicks(-1);

                // Autos lavados por sede (citas finalizadas)
                adminVm.AutosPorSede = db.sede
                    .Where(s => s.sedeid != 0)
                    .Select(s => new
                    {
                        s.nombre,
                        Cantidad = s.citas.Count(c => c.estado == 3
                                                      && c.fechahorafin >= hoyInicio
                                                      && c.fechahorafin <= hoyFin)
                    })
                    .ToDictionary(x => x.nombre, x => x.Cantidad);

                // Top 3 empleados mejor calificados (promedio de resenas)
                adminVm.TopEmpleados = db.empleado
                    .Select(e => new
                    {
                        NombreCompleto = e.usuario.nombre + " " + e.usuario.apellido,
                        Promedio = e.resenas.Any() ? e.resenas.Average(r => r.puntuacion) : 0
                    })
                    .OrderByDescending(x => x.Promedio)
                    .Take(3)
                    .AsEnumerable()
                    .Select(x => $"{x.NombreCompleto} ({x.Promedio:F1})")
                    .ToList();




                // Inicializamos listas
                adminVm.DiasUltimos7Dias = new List<string>();
                adminVm.IngresosUltimos7Dias = new List<decimal>();

                // Fecha de hoy
                var hoy = DateTime.Today;

                for (int i = 6; i >= 0; i--)
                {
                    var dia = hoy.AddDays(-i);
                    var diaInicio = dia.Date;
                    var diaFin = diaInicio.AddDays(1).AddTicks(-1);

                    
                    var citasDelDia = db.cita
                        .Where(c => c.estado == 3
                                    && c.fechahorafin >= diaInicio
                                    && c.fechahorafin <= diaFin)
                        .ToList();

                    decimal ingresosDia = 0;

                    if (citasDelDia.Any())
                    {
                        var tiposervicios = db.tiposervicio.ToList();

                        ingresosDia = citasDelDia
                            .Join(tiposervicios,
                                  c => c.tiposervicioid,
                                  t => t.tiposervicioid,
                                  (c, t) => t.precio)
                            .Sum();
                    }

                    adminVm.DiasUltimos7Dias.Add(diaInicio.ToString("dddd"));
                    adminVm.IngresosUltimos7Dias.Add(ingresosDia);
                }

                adminVm.IngresosHoy = adminVm.IngresosUltimos7Dias.Any() ? adminVm.IngresosUltimos7Dias.Last() : 0;

                vm.AdminData = adminVm;
            }
            if (rol == "Empleado")
            {
                var empleadoVm = new DashboardEmpleadoViewModel();
                var empleadoId = (long)Session["UsuarioId"];
                var hoyInicio = DateTime.Today;
                var hoyFin = hoyInicio.AddDays(1).AddTicks(-1);

                // 3 citas más recientes asignadas hoy
                empleadoVm.CitasAsignadasHoy = db.cita
                    .Where(c => c.empleado.usuarioid == empleadoId && c.estado == 1 && c.fechahorainicio >= hoyInicio && c.fechahorainicio <= hoyFin)
                    .OrderBy(c => c.fechahorainicio)
                    .Take(3)
                    .Select(c => new CitaCard
                    {
                        Cliente = c.cliente.usuario.nombre + " " + c.cliente.usuario.apellido,
                        Vehiculo = c.vehiculo.placa,
                        Servicio = c.tiposervicio.nombre,
                        HoraInicio = c.fechahorainicio,
                        HoraFin = c.fechahorafin
                    })
                    .ToList();

                // Citas finalizadas hoy
                empleadoVm.CitasFinalizadasHoy = db.cita
                    .Where(c => c.empleado.usuarioid == empleadoId && c.estado == 3 && c.fechahorafin >= hoyInicio && c.fechahorafin <= hoyFin)
                    .OrderBy(c => c.fechahorainicio)
                    .Take(3)
                    .Select(c => new CitaCard
                    {
                        Cliente = c.cliente.usuario.nombre + " " + c.cliente.usuario.apellido,
                        Vehiculo = c.vehiculo.placa,
                        Servicio = c.tiposervicio.nombre,
                        HoraInicio = c.fechahorainicio,
                        HoraFin = c.fechahorafin
                    })
                    .ToList();

                // Historial últimos 7 días (citas finalizadas por día)
                empleadoVm.DiasUltimos7Dias = new List<string>();
                empleadoVm.CitasUltimos7Dias = new List<int>();

                var hoy = DateTime.Today;
                for (int i = 6; i >= 0; i--)
                {
                    var dia = hoy.AddDays(-i);
                    var diaInicio = dia.Date;
                    var diaFin = diaInicio.AddDays(1).AddTicks(-1);

                    int totalCitas = db.cita.Count(c => c.empleado.usuarioid == empleadoId
                                                       && c.estado == 3
                                                       && c.fechahorafin >= diaInicio
                                                       && c.fechahorafin <= diaFin);

                    empleadoVm.DiasUltimos7Dias.Add(dia.ToString("dddd"));
                    empleadoVm.CitasUltimos7Dias.Add(totalCitas);
                }

                vm.EmpleadoData = empleadoVm;
            }
            else if (rol == "Cliente")
            {
                var clienteId = (long)Session["UsuarioId"];
                var cliente = db.cliente.FirstOrDefault(c => c.usuarioid == clienteId);

                var clienteVm = new DashboardClienteViewModel();

                if (cliente != null)
                {
                    // --- Últimas 3 citas finalizadas ---
                    clienteVm.UltimasCitas = db.cita
                        .Where(c => c.clienteid == cliente.clienteid && c.estado == 3)
                        .OrderByDescending(c => c.fechahorafin)
                        .Take(3)
                        .Select(c => new CitaClienteCard
                        {
                            Sede = c.sede.nombre,
                            Servicio = c.tiposervicio.nombre,
                            FechaHoraInicio = c.fechahorainicio,
                            FechaHoraFin = c.fechahorafin ?? c.fechahorainicio,
                            Vehiculo = c.vehiculo.placa
                        })
                        .ToList();

                    // --- Próxima cita programada ---
                    clienteVm.ProximaCita = db.cita
                        .Where(c => c.clienteid == cliente.clienteid && c.estado == 1 && c.fechahorainicio > DateTime.Now)
                        .OrderBy(c => c.fechahorainicio)
                        .Select(c => new CitaClienteCard
                        {
                            Sede = c.sede.nombre,
                            Servicio = c.tiposervicio.nombre,
                            FechaHoraInicio = c.fechahorainicio,
                            FechaHoraFin = c.fechahorafin ?? c.fechahorainicio,
                            Vehiculo = c.vehiculo.placa
                        })
                        .FirstOrDefault();

                    // --- Sede más cercana ---
                    if (cliente.latitud != 0 && cliente.longitud != 0)
                    {
                        var sedes = db.sede.ToList();

                        var geo = new Utilities.Location.Location();

                        var sedeCercana = sedes
                            .Select(s => new
                            {
                                s.nombre,
                                s.latitud,
                                s.longitud,
                                Distancia = geo.CalcularDistancia(cliente.latitud, cliente.longitud, s.latitud, s.longitud)
                            })
                            .OrderBy(x => x.Distancia)
                            .FirstOrDefault();

                        if (sedeCercana != null)
                        {
                            clienteVm.SedeMasCercana = sedeCercana.nombre;
                            clienteVm.LatitudSede = sedeCercana.latitud;
                            clienteVm.LongitudSede = sedeCercana.longitud;
                            clienteVm.DistanciaSedeKm = sedeCercana.Distancia;
                        }
                    }
                }
                vm.ClienteData = clienteVm;
            }

            ViewBag.Rol = rol;
            return View(vm);
        }
    }
}