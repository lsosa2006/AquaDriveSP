using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using AquaDriveSP.Autentication;

namespace AquaDriveSP.Controllers
{
    [AuthorizeRole("Admin")]
    public class AdminController : Controller
    {
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
    }
}