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
        #endregion

        #region Funciones
        
        #endregion
    }
}