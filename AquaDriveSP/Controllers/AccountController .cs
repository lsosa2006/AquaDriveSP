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
    public class AccountController : Controller
    {
        private readonly AppDbContext _db = new AppDbContext();

        // ---------------------------
        // Registro
        // ---------------------------
        [HttpPost]
        public ActionResult ResgistrarUsuario(long usuarioId, string nombre, string apellido, string correo, string telefono, string contrasena, string tipoCuenta)
        {
            if (_db.Usuarios.Any(u => u.Email == correo))
            {
                return Json(new { exito = false, mensaje = "El correo ya está registrado." });
            }

            if (_db.Usuarios.Any(u => u.UsuarioId == usuarioId))
            {
                return Json(new { exito = false, mensaje = "El numero de documento ya está registrado." });
            }

            // Crear usuario base
            var usuario = new Usuario
            {
                UsuarioId = usuarioId,
                Nombre = nombre,
                Apellido = apellido,
                Email = correo,
                Telefono = telefono,
                Contrasena = contrasena,
                FechaCreacion = DateTime.Now
            };

            _db.Usuarios.Add(usuario);
            _db.SaveChanges();

            // Crear tipo de cuenta según selección
            switch (tipoCuenta.ToLower())
            {
                case "cliente":
                    _db.Clientes.Add(new Cliente
                    {
                        UsuarioId = usuario.UsuarioId
                    });
                    break;

                case "empleado":
                    _db.Empleados.Add(new Empleado
                    {
                        UsuarioId = usuario.UsuarioId,
                        SedeId = 1, // temporal, se puede actualizar
                        Estado = "Pendiente" // requiere aprobación de admin
                    });
                    break;

                case "admin":
                    _db.Administradores.Add(new Administrador
                    {
                        UsuarioId = usuario.UsuarioId,
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
        public ActionResult IniciarSesion(long usuarioId, string contrasena, string tipoCuenta)
        {

            switch (tipoCuenta.ToLower())
            {
                case "cliente":
                    var cliente = _db.Clientes.FirstOrDefault(c => c.Usuario.UsuarioId == usuarioId && c.Usuario.Contrasena == contrasena);
                    if (cliente != null) return Json(new { exito = true, mensaje = "Inicio de sesión exitoso", id = cliente.ClienteId });
                    break;

                case "empleado":
                    var empleado = _db.Empleados.FirstOrDefault(e => e.Usuario.UsuarioId == usuarioId && e.Usuario.Contrasena == contrasena);
                    if (empleado != null)
                    {
                        if (empleado.Estado != "Aceptado")
                            return Json(new { exito = false, mensaje = "Empleado aún no aprobado por un administrador." });
                        return Json(new { exito = true, mensaje = "Inicio de sesión exitoso", id = empleado.EmpleadoId });
                    }
                    break;

                case "admin":
                    var admin = _db.Administradores.FirstOrDefault(a => a.Usuario.UsuarioId == usuarioId && a.Usuario.Contrasena == contrasena);
                    if (admin != null)
                    {
                        if (admin.Estado != "Aceptado")
                            return Json(new { exito = false, mensaje = "Administrador aún no aprobado por otro admin." });
                        return Json(new { exito = true, mensaje = "Inicio de sesión exitoso", id = admin.AdministradorId });
                    }
                    break;
            }

            return Json(new { exito = false, mensaje = "Credenciales inválidas." });
        }

        // ---------------------------
        // Recuperación de contraseña
        // ---------------------------
        [HttpPost]
        public ActionResult RecuperarContrasena(long usuarioId, string correo)
        {
            var usuario = _db.Usuarios.FirstOrDefault(u => u.Email == correo && u.UsuarioId == usuarioId);
            if (usuario == null)
                return Json(new { exito = false, mensaje = "Correo no registrado." });

            string nuevaContrasena = GenerarContrasenaAleatoria();
            usuario.Contrasena = nuevaContrasena;
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