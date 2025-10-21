using AquaDriveSP.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;
using System.Web.Security;

namespace AquaDriveSP.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();
        #region Vistas
        public ActionResult IniciarSesion()
        {
            return View();
        }

        public ActionResult Registro()
        {
            return View();
        }

        public ActionResult RecuperarContrasena()
        {
            return View();
        }
        #endregion
        #region Funciones
        // Registrar usuario
        [HttpPost]
        public ActionResult Registro(long usuarioId, string nombre, string apellido, string correo, string telefono, string contrasena, string direccion, string tipoCuenta)
        {
            try
            {
                string mensaje = "";
                if (db.usuario.Any(u => u.email == correo))
                    return Json(new { exito = false, mensaje = "El correo ya está registrado." });

                if (db.usuario.Any(u => u.usuarioid == usuarioId))
                    return Json(new { exito = false, mensaje = "El número de documento ya está registrado." });

                var usuario = new Usuario
                {
                    usuarioid = usuarioId,
                    nombre = nombre,
                    apellido = apellido,
                    email = correo,
                    telefono = telefono,
                    contrasena = contrasena,
                    direccion = direccion,
                    fechacreacion = DateTime.Now
                };

                // Crear tipo de cuenta según selección
                switch (tipoCuenta.ToLower())
                {
                    case "cliente":
                        var geo = new Utilities.Location.Location();
                        var coordenadas = geo.ObtenerCoordenadas(direccion);
                        var cliente = new Cliente
                        {
                            usuarioid = usuario.usuarioid,
                            latitud = (double)(coordenadas?.lat ?? 0.0),
                            longitud = (double)(coordenadas?.lng ?? 0.0)
                        };
                        db.usuario.Add(usuario);
                        db.cliente.Add(cliente);
                        mensaje = "Usuario registrado correctamente.";
                        break;

                    case "empleado":
                        db.usuario.Add(usuario);
                        db.empleado.Add(new Empleado
                        {
                            usuarioid = usuario.usuarioid,
                            sedeid = 0, // temporal, se puede actualizar
                            estado = 0
                        });
                        mensaje = "Usuario registrado correctamente, espera aprobación.";
                        break;

                    case "admin":
                        db.usuario.Add(usuario);
                        db.administrador.Add(new Administrador
                        {
                            usuarioid = usuario.usuarioid
                        });
                        mensaje = "Usuario registrado correctamente.";
                        break;

                    default:
                        return Json(new { exito = false, mensaje = "Tipo de cuenta inválido." });
                }

                db.SaveChanges();
                return Json(new { exito = true, mensaje = mensaje });
            }
            catch (Exception ex)
            {
                return Json(new { exito = false, mensaje = $"Error al registrar usuario: {ex.Message}" });
            }
        }

        // Inicio de sesión
        [HttpPost]
        public ActionResult IniciarSesion(long usuarioId, string contrasena)
        {
            try
            {
                var usuario = db.usuario.FirstOrDefault(u => u.usuarioid == usuarioId);

                if (usuario == null)
                {
                    return Json(new { exito = false, mensaje = "El usuario no existe." });
                }

                if (usuario.contrasena != contrasena)
                {
                    return Json(new { exito = false, mensaje = "La contraseña es incorrecta." });
                }

                string rol;

                // Cliente
                var cliente = db.cliente.FirstOrDefault(c => c.usuarioid == usuario.usuarioid);
                if (cliente != null)
                {
                    rol = "Cliente";
                    FormsAuthentication.SetAuthCookie(usuario.usuarioid.ToString(), false);
                    // Guardar rol en Session (opcional)
                    Session["UsuarioId"] = usuario.usuarioid;
                    Session["Rol"] = rol;
                    var vehiculos = db.vehiculo
                        .Where(v => v.cliente.usuarioid == usuario.usuarioid)
                        .Select(v => new
                        {
                            v.placa
                        })
                        .ToList();
                    Session["HasVehicle"] = vehiculos.Count > 0 ? true : false;
                    return Json(new
                    {
                        exito = true,
                        mensaje = "Inicio de sesión exitoso",
                        tipoCuenta = "cliente",
                        id = cliente.clienteid
                    });
                }

                // Empleado
                var empleado = db.empleado.FirstOrDefault(e => e.usuarioid == usuario.usuarioid);
                if (empleado != null)
                {
                    if (empleado.estado != 1)
                        return Json(new { exito = false, mensaje = "Empleado aún no aprobado por un administrador." });

                    rol = "Empleado";
                    FormsAuthentication.SetAuthCookie(usuario.usuarioid.ToString(), false);
                    // Guardar rol en Session (opcional)
                    Session["UsuarioId"] = usuario.usuarioid;
                    Session["Rol"] = rol;
                    return Json(new
                    {
                        exito = true,
                        mensaje = "Inicio de sesión exitoso (Empleado)",
                        tipoCuenta = "empleado",
                        id = empleado.empleadoid
                    });
                }

                // Administrador
                var admin = db.administrador.FirstOrDefault(a => a.usuarioid == usuario.usuarioid);
                if (admin != null)
                {
                    rol = "Admin";
                    FormsAuthentication.SetAuthCookie(usuario.usuarioid.ToString(), false);
                    // Guardar rol en Session (opcional)
                    Session["UsuarioId"] = usuario.usuarioid;
                    Session["Rol"] = rol;
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

        // Recuperación de contraseña
        [HttpPost]
        public ActionResult RecuperarContrasena(long usuarioId, string correo)
        {
            var usuario = db.usuario.FirstOrDefault(u => u.email == correo && u.usuarioid == usuarioId);
            if (usuario == null)
                return Json(new { exito = false, mensaje = "Correo no registrado." });

            string nuevaContrasena = GenerarContrasenaAleatoria();
            usuario.contrasena = nuevaContrasena;
            db.SaveChanges();

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

        // Función auxiliar
        private string GenerarContrasenaAleatoria(int longitud = 8)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var rnd = new Random();
            return new string(Enumerable.Repeat(chars, longitud)
                .Select(s => s[rnd.Next(s.Length)]).ToArray());
        }
        #endregion
    }
}