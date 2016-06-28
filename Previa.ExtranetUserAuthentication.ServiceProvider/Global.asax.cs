using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using Bootstrap;
using Bootstrap.Autofac;
using Bootstrap.Extensions.StartupTasks;
using log4net;

namespace Previa.ExtranetUserAuthentication.ServiceProvider
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected void Application_Start()
        {
            Log.Debug("Application starting...");

            Bootstrapper.With
                .Autofac()
                .And
                .StartupTasks()
                .Start();

            Log.Debug("Application started.");
        }

        protected void Application_Error()
        {
            var error = Server.GetLastError();
            if (error != null)
            {
                Log.Error("Unhandled exception.", error.GetBaseException());
            }
        }
    }
}