using AquaDriveSP.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;

namespace AquaDriveSP.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _db = new AppDbContext();

        // GET: /Login/IniciarSesion
        public ActionResult IniciarSesion()
        {
            return View();
        }

        // GET: /Login/Registro
        public ActionResult Registro()
        {
            return View();
        }

        // GET: /Login/RecuperarContrasena
        public ActionResult RecuperarContrasena()
        {
            return View();
        }

        // ---------------------------
        // Registro
        // ---------------------------
        [HttpPost]
        public ActionResult Registro(long usuarioId, string nombre, string apellido, string correo, string telefono, string contrasena, string tipoCuenta)
        {
            try
            {
                // Validaciones
                if (_db.usuario.Any(u => u.email == correo))
                    return Json(new { exito = false, mensaje = "El correo ya está registrado." });

                if (_db.usuario.Any(u => u.usuarioid == usuarioId))
                    return Json(new { exito = false, mensaje = "El número de documento ya está registrado." });

                // Crear usuario base
                var usuario = new Usuario
                {
                    usuarioid = usuarioId,
                    nombre = nombre,
                    apellido = apellido,
                    email = correo,
                    telefono = telefono,
                    contrasena = contrasena,
                    fechacreacion = DateTime.Now
                };

                _db.usuario.Add(usuario);
                _db.SaveChanges();

                // Crear tipo de cuenta según selección
                switch (tipoCuenta.ToLower())
                {
                    case "cliente":
                        _db.cliente.Add(new Cliente { usuarioid = usuario.usuarioid });
                        break;

                    case "empleado":
                        _db.empleado.Add(new Empleado
                        {
                            usuarioid = usuario.usuarioid,
                            sedeid = 1, // temporal, se puede actualizar
                            estado = "Pendiente"
                        });
                        break;

                    case "admin":
                        _db.administrador.Add(new Administrador
                        {
                            usuarioid = usuario.usuarioid,
                            estado = "Pendiente"
                        });
                        break;

                    default:
                        return Json(new { exito = false, mensaje = "Tipo de cuenta inválido." });
                }

                _db.SaveChanges();
                return Json(new { exito = true, mensaje = "Usuario registrado correctamente. Espera aprobación si es empleado o admin." });
            }
            catch (Exception ex)
            {
                return Json(new { exito = false, mensaje = $"Error al registrar usuario: {ex.Message}" });
            }
        }

        // ---------------------------
        // Inicio de sesión
        // ---------------------------
        [HttpPost]
        public ActionResult IniciarSesion(long usuarioId, string contrasena)
        {
            try
            {
                var usuario = _db.usuario.FirstOrDefault(u => u.usuarioid == usuarioId && u.contrasena == contrasena);
                if (usuario == null)
                    return Json(new { exito = false, mensaje = "Credenciales inválidas." });

                // Cliente
                var cliente = _db.cliente.FirstOrDefault(c => c.usuarioid == usuario.usuarioid);
                if (cliente != null)
                {
                    return Json(new
                    {
                        exito = true,
                        mensaje = "Inicio de sesión exitoso (Cliente)",
                        tipoCuenta = "cliente",
                        id = cliente.clienteid
                    });
                }

                // Empleado
                var empleado = _db.empleado.FirstOrDefault(e => e.usuarioid == usuario.usuarioid);
                if (empleado != null)
                {
                    if (empleado.estado != "Aceptado")
                        return Json(new { exito = false, mensaje = "Empleado aún no aprobado por un administrador." });

                    return Json(new
                    {
                        exito = true,
                        mensaje = "Inicio de sesión exitoso (Empleado)",
                        tipoCuenta = "empleado",
                        id = empleado.empleadoid
                    });
                }

                // Administrador
                var admin = _db.administrador.FirstOrDefault(a => a.usuarioid == usuario.usuarioid);
                if (admin != null)
                {
                    if (admin.estado != "Aceptado")
                        return Json(new { exito = false, mensaje = "Administrador aún no aprobado por otro admin." });

                    return Json(new
                    {
                        exito = true,
                        mensaje = "Inicio de sesión exitoso (Admin)",
                        tipoCuenta = "admin",
                        id = admin.administradorid
                    });
                }

                return Json(new { exito = false, mensaje = "El usuario no tiene un rol asignado." });
            }
            catch (Exception ex)
            {
                return Json(new { exito = false, mensaje = $"Error al iniciar sesión: {ex.Message}" });
            }
        }

        // ---------------------------
        // Recuperación de contraseña
        // ---------------------------
        [HttpPost]
        public ActionResult RecuperarContrasena(long usuarioId, string correo)
        {
            var usuario = _db.usuario.FirstOrDefault(u => u.email == correo && u.usuarioid == usuarioId);
            if (usuario == null)
                return Json(new { exito = false, mensaje = "Correo no registrado." });

            string nuevaContrasena = GenerarContrasenaAleatoria();
            usuario.contrasena = nuevaContrasena;
            _db.SaveChanges();

            try
            {
                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential("miguelnal0706@gmail.com", "viwscmvvvliwmrib");
                    smtp.EnableSsl = true;

                    var mail = new MailMessage();
                    mail.From = new MailAddress("miguelnal0706@gmail.com", "AquaDriveSP");
                    mail.To.Add(correo);
                    mail.Subject = "Recuperación de contraseña AquaDriveSP";
                    mail.Body = $"Tu nueva contraseña es: {nuevaContrasena}";
                    mail.IsBodyHtml = false;

                    smtp.Send(mail);
                }

                return Json(new { exito = true, mensaje = "Nueva contraseña enviada al correo." });
            }
            catch (Exception ex)
            {
                return Json(new { exito = false, mensaje = "Error al enviar el correo: " + ex.Message });
            }
        }

        // ---------------------------
        // Función auxiliar
        // ---------------------------
        private string GenerarContrasenaAleatoria(int longitud = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var rnd = new Random();
            return new string(Enumerable.Repeat(chars, longitud)
                .Select(s => s[rnd.Next(s.Length)]).ToArray());
        }
    }
}