using AquaDriveSP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace AquaDriveSP
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            Task.Run(() => CalentarDb());
        }

        private void CalentarDb()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    // Ejecutamos un query ligero para inicializar la conexión
                    db.Database.SqlQuery<int>("SELECT 1;").FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al calentar la DB: " + ex.Message);
            }
        }
    }
}
