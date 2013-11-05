using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Data.Entity;
using Sweaty_T_Shirt.Models;
using Sweaty_T_Shirt.DAL;
using WebMatrix.WebData;
using System.Configuration;

namespace Sweaty_T_Shirt
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();


            bool runInitializer = Convert.ToBoolean(ConfigurationManager.AppSettings["RunDatabaseInitializer"]);
            if (runInitializer)
            {
                Database.SetInitializer<SweatyTShirtContext>(new SweatyTShirtInitializer());
                using (SweatyTShirtContext sweatyTShirtContext = new SweatyTShirtContext())
                {
                    sweatyTShirtContext.Database.Initialize(true);
                }
            }
            if (!WebSecurity.Initialized)
            {
                WebSecurity.InitializeDatabaseConnection("SweatyTShirtContext", "UserProfile", "UserId", "UserName", autoCreateTables: true);
            }

        }
    }
}