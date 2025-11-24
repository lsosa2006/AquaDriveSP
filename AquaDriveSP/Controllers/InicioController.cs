using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static System.Collections.Specialized.BitVector32;
using System.Web.Mvc;
using System.Web.Security;
using AquaDriveSP.Autentication;
using AquaDriveSP.Models;

namespace AquaDriveSP.Controllers
{
    public class InicioController : Controller
    {
        private AppDbContext db = new AppDbContext();
        public ActionResult Index()
        {
            var servicios = db.tiposervicio.ToList();
            return View(servicios);
        }
    }
}