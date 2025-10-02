using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AquaDriveSP.Autentication
{
    public class AuthorizeRoleAttribute : AuthorizeAttribute
    {
        private readonly string[] allowedRoles;
        public AuthorizeRoleAttribute(params string[] roles)
        {
            allowedRoles = roles;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var sessionUser = httpContext.Session["Rol"];
            if (sessionUser == null)
                return false;

            string role = sessionUser.ToString();
            return Array.Exists(allowedRoles, r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // Redirige al login si no está autorizado
            filterContext.Result = new RedirectResult("~/Login/IniciarSesion");
        }
    }
}