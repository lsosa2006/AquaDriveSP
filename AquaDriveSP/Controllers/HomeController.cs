using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AquaDriveSP.Autentication;


namespace AquaDriveSP.Controllers
{
    [AuthorizeRole("Admin", "Empleado", "Cliente")]
    public class HomeController : Controller
    {
        public ActionResult Dashboard()
        {
            string rol = Session["Rol"]?.ToString() ?? "Invitado";

            // Puedes crear un modelo o usar ViewBag/ViewData
            ViewBag.Rol = rol;

            switch (rol)
            {
                case "Admin":
                    ViewBag.TopEmpleados = new List<string> { "Daniel", "Sebastian", "Juan" };
                    ViewBag.Sedes = new Dictionary<string, int>
                {
                    { "Arkadia", 8 },
                    { "Florida", 5 },
                    { "Belen", 11 }
                };
                    ViewBag.Estadisticas = new { AutosHoy = 35, Ingresos = 1200000 };
                    break;

                case "Empleado":
                    ViewBag.MisTareas = new List<string> { "Lavar auto 1", "Lavar auto 2" };
                    ViewBag.HorasTrabajo = 8;
                    break;

                case "Cliente":
                    ViewBag.MisCitas = new List<string> { "Cita 1: 10:00am", "Cita 2: 3:00pm" };
                    break;
            }

            return View();
        }
    }
}