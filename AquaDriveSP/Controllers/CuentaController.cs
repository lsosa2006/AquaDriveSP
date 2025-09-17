using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static System.Collections.Specialized.BitVector32;
using System.Web.Mvc;
using System.Web.Security;
using AquaDriveSP.Autentication;

namespace AquaDriveSP.Controllers
{
    [AuthorizeRole("Admin", "Empleado", "Cliente")]
    public class CuentaController : Controller
    {
        public ActionResult CerrarSesion()
        {
            // 1. Limpiar la sesión
            Session.Clear();
            Session.Abandon();

            // 2. Limpiar la cookie de autenticación
            FormsAuthentication.SignOut();

            // 3. Redirigir al login
            return RedirectToAction("IniciarSesion", "Login");
        }
    }
}