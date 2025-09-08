using AquaDriveSP.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace AquaDriveSP.Controllers
{
    public class LoginController : Controller
    {
        // GET: /Home/Login
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
        private readonly AppDbContext _db = new AppDbContext();

        // ---------------------------
        // Registro
        // ---------------------------
        [HttpPost]
        public ActionResult Registro(long usuarioId, string nombre, string apellido, string correo, string telefono, string contrasena, string tipoCuenta)
        {
            if (_db.usuario.Any(u => u.email == correo))
            {
                return Json(new { exito = false, mensaje = "El correo ya está registrado." });
            }

            if (_db.usuario.Any(u => u.usuarioid == usuarioId))
            {
                return Json(new { exito = false, mensaje = "El numero de documento ya está registrado." });
            }

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
                    _db.cliente.Add(new Cliente
                    {
                        usuarioid = usuario.usuarioid
                    });
                    break;

                case "empleado":
                    _db.empleado.Add(new Empleado
                    {
                        UsuarioId = usuario.usuarioid,
                        SedeId = 1, // temporal, se puede actualizar
                        Estado = "Pendiente" // requiere aprobación de admin
                    });
                    break;

                case "admin":
                    _db.administrador.Add(new Administrador
                    {
                        UsuarioId = usuario.usuarioid,
                        Estado = "Pendiente" // requiere aprobación de otro admin
                    });
                    break;

                default:
                    return Json(new { exito = false, mensaje = "Tipo de cuenta inválido." });
            }

            _db.SaveChanges();
            return Json(new { exito = true, mensaje = "Usuario registrado correctamente. Espera aprobación si es empleado o admin." });
        }

        // ---------------------------
        // Inicio de sesión
        // ---------------------------
        [HttpPost]
        public ActionResult IniciarSesion(long usuarioId, string contrasena)
        {
            // Paso 1: Buscar el usuario
            var usuario = _db.usuario.FirstOrDefault(u => u.usuarioid == usuarioId && u.contrasena == contrasena);

            if (usuario == null)
            {
                return Json(new { exito = false, mensaje = "Credenciales inválidas." });
            }

            // Paso 2: Validar si es cliente
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

            // Paso 3: Validar si es empleado
            var empleado = _db.empleado.FirstOrDefault(e => e.UsuarioId == usuario.usuarioid);
            if (empleado != null)
            {
                if (empleado.Estado != "Aceptado")
                    return Json(new { exito = false, mensaje = "Empleado aún no aprobado por un administrador." });

                return Json(new
                {
                    exito = true,
                    mensaje = "Inicio de sesión exitoso (Empleado)",
                    tipoCuenta = "empleado",
                    id = empleado.EmpleadoId
                });
            }

            // Paso 4: Validar si es administrador
            var admin = _db.administrador.FirstOrDefault(a => a.UsuarioId == usuario.usuarioid);
            if (admin != null)
            {
                if (admin.Estado != "Aceptado")
                    return Json(new { exito = false, mensaje = "Administrador aún no aprobado por otro admin." });

                return Json(new
                {
                    exito = true,
                    mensaje = "Inicio de sesión exitoso (Admin)",
                    tipoCuenta = "admin",
                    id = admin.AdministradorId
                });
            }
            return Json(new { exito = false, mensaje = "El usuario no tiene un rol asignado." });
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
                // Enviar email (requiere configuración SMTP)
                using (var smtp = new SmtpClient())
                {
                    var mail = new MailMessage();
                    mail.To.Add(correo);
                    mail.Subject = "Recuperación de contraseña AquaDriveSP";
                    mail.Body = $"Tu nueva contraseña es: {nuevaContrasena}";
                    smtp.Send(mail);
                }
                return Json(new { exito = true, mensaje = "Nueva contraseña enviada al correo." });
            }
            catch
            {
                return Json(new { exito = false, mensaje = "Error al enviar el correo." });
            }
        }

        // ---------------------------
        // Funciones auxiliares
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