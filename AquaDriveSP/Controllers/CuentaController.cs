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
    [AuthorizeRole("Admin", "Empleado", "Cliente")]
    public class CuentaController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();
        public ActionResult EditarDatos()
        {
            try
            {
                long usuarioId = (long)Session["UsuarioId"];
                var usuario = db.usuario.FirstOrDefault(u => u.usuarioid == usuarioId);

                if (usuario == null)
                    return HttpNotFound();

                return View(usuario);
            }
            catch (Exception ex)
            {
                // Podrías mostrar una vista de error si algo falla
                ViewBag.Error = $"Error al cargar los datos: {ex.Message}";
                return View();
            }
        }
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
        public ActionResult CambiarContrasena()
        {
            try
            {
                long usuarioId = (long)Session["UsuarioId"];
                var usuario = db.usuario.FirstOrDefault(u => u.usuarioid == usuarioId);

                if (usuario == null)
                    return HttpNotFound();

                return View(usuario);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error al cargar los datos: {ex.Message}";
                return View();
            }
        }

        [HttpPost]
        public JsonResult ActualizarDatosPersonales(Usuario usuario)
        {
            try
            {
                var usuarioDb = db.usuario.FirstOrDefault(u => u.usuarioid == usuario.usuarioid);
                if (usuarioDb == null)
                    return Json(new { success = false, message = "Usuario no encontrado" });

                // Actualizar campos editables
                usuarioDb.nombre = usuario.nombre;
                usuarioDb.apellido = usuario.apellido;
                usuarioDb.email = usuario.email;
                usuarioDb.telefono = usuario.telefono;

                db.SaveChanges();

                return Json(new { success = true, message = "Datos actualizados correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al actualizar datos: {ex.Message}" });
            }
        }


        
        [HttpPost]
        public JsonResult CambiarContrasena(long usuarioid, string contrasenaActual, string nuevaContrasena, string confirmarContrasena)
        {
            try
            {
                var usuarioDb = db.usuario.FirstOrDefault(u => u.usuarioid == usuarioid);
                if (usuarioDb == null)
                    return Json(new { success = false, message = "Usuario no encontrado" });

                if (string.IsNullOrWhiteSpace(contrasenaActual) ||
                    string.IsNullOrWhiteSpace(nuevaContrasena) ||
                    string.IsNullOrWhiteSpace(confirmarContrasena))
                {
                    return Json(new { success = false, message = "Todos los campos son obligatorios" });
                }

                if (usuarioDb.contrasena != contrasenaActual)
                    return Json(new { success = false, message = "La contraseña actual no es correcta" });

                if (nuevaContrasena != confirmarContrasena)
                    return Json(new { success = false, message = "Las contraseñas no coinciden" });

                if (nuevaContrasena.Length < 6)
                    return Json(new { success = false, message = "La nueva contraseña debe tener al menos 6 caracteres" });

                usuarioDb.contrasena = nuevaContrasena;
                db.SaveChanges();

                return Json(new { success = true, message = "Tu contraseña ha sido cambiada exitosamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al cambiar la contraseña: {ex.Message}" });
            }
        }
    }
}