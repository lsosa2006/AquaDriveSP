using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AquaDriveSP.Autentication;
using AquaDriveSP.Models;
using AquaDriveSP.ViewModels;


namespace AquaDriveSP.Controllers
{
    [AuthorizeRole("Admin", "Empleado", "Cliente")]
    public class HomeController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        public ActionResult Dashboard()
        {
            string rol = Session["Rol"]?.ToString() ?? "Invitado";

            if (rol == "Admin")
            {
                var vm = new DashboardAdminViewModel();
                var hoyInicio = DateTime.Today;
                var hoyFin = hoyInicio.AddDays(1).AddTicks(-1);
                // ---------------------------
                // 1. Autos lavados por sede (citas finalizadas)
                // ---------------------------
                vm.AutosPorSede = db.sede
                    .Where(s => s.sedeid != 0)
                    .Select(s => new
                    {
                        s.nombre,
                        Cantidad = s.citas.Count(c => c.estado == "Finalizada"
                                                      && c.fechahorafin >= hoyInicio
                                                      && c.fechahorafin <= hoyFin)
                    })
                    .ToDictionary(x => x.nombre, x => x.Cantidad);

                // ---------------------------
                // 2. Top 3 empleados mejor calificados (promedio de resenas)
                // ---------------------------
                vm.TopEmpleados = db.empleado
                    .Select(e => new
                    {
                        NombreCompleto = e.usuario.nombre + " " + e.usuario.apellido,
                        Promedio = e.resenas.Any() ? e.resenas.Average(r => r.puntuacion) : 0
                    })
                    .OrderByDescending(x => x.Promedio)
                    .Take(3)
                    .AsEnumerable() // <- Traemos a memoria
                    .Select(x => $"{x.NombreCompleto} ({x.Promedio:F1})") // <- Formateo ya en C#
                    .ToList();




                // Inicializamos listas
                vm.DiasUltimos7Dias = new List<string>();
                vm.IngresosUltimos7Dias = new List<decimal>();

                // Fecha de hoy
                var hoy = DateTime.Today;

                for (int i = 6; i >= 0; i--)
                {
                    var dia = hoy.AddDays(-i);
                    var diaInicio = dia.Date;
                    var diaFin = diaInicio.AddDays(1).AddTicks(-1);

                    // Traemos las citas finalizadas del día a memoria, puede estar vacío
                    var citasDelDia = db.cita
                        .Where(c => c.estado == "Finalizada"
                                    && c.fechahorafin >= diaInicio
                                    && c.fechahorafin <= diaFin)
                        .ToList();

                    decimal ingresosDia = 0;

                    if (citasDelDia.Any())
                    {
                        // Traemos tiposervicio a memoria solo si hay citas
                        var tiposervicios = db.tiposervicio.ToList();

                        ingresosDia = citasDelDia
                            .Join(tiposervicios,
                                  c => c.tiposervicioid,
                                  t => t.tiposervicioid,
                                  (c, t) => t.precio)
                            .Sum();
                    }

                    vm.DiasUltimos7Dias.Add(diaInicio.ToString("dddd")); // Lunes, Martes...
                    vm.IngresosUltimos7Dias.Add(ingresosDia);
                }

                // Ingresos hoy: última posición de la lista, seguro incluso si está vacía
                vm.IngresosHoy = vm.IngresosUltimos7Dias.Any() ? vm.IngresosUltimos7Dias.Last() : 0;

                return View(vm);
            }

            // ---------------------------
            // Otros roles
            // ---------------------------
            if (rol == "Empleado")
            {
                ViewBag.MisTareas = new List<string> { "Lavar auto 1", "Lavar auto 2" };
                ViewBag.HorasTrabajo = 8;
            }
            else if (rol == "Cliente")
            {
                ViewBag.MisCitas = new List<string> { "Cita 1: 10:00am", "Cita 2: 3:00pm" };
            }

            ViewBag.Rol = rol;
            return View();
        }
    }
}