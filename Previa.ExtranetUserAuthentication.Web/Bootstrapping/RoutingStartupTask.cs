using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using Bootstrap.Extensions.StartupTasks;
using log4net;

namespace Previa.ExtranetUserAuthentication.Web.Bootstrapping
{
    public class RoutingStartupTask : IStartupTask
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Reset()
        {
            RouteTable.Routes.Clear();
        }

        public void Run()
        {
            Log.Debug("Registering routes...");
            RegisterRoutes(RouteTable.Routes);
        }

        public void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });

            routes.MapRoute(
                "LocalUser",
                "LocalUser",
                new { controller = "QlikView", action = "LocalUser" });

            //TODO - remove this route
            //routes.MapRoute(
            //    "TestLogin",
            //    "TestLogin",
            //    new { controller = "QlikView", action = "TestLogin" });

            //SignIn not planned to be used, however many request comes to this URL
            //Not sure why it is so, but maybe it has been communicated to QlikView...
            //Map the route and get rid of the errors in the log files...
            routes.MapRoute(
                "SignIn",
                "SignIn",
                new { controller = "QlikView", action = "SignIn" });

            routes.MapRoute(
                "SignOut",
                "SignOut",
                new { controller = "QlikView", action = "SignOut" });

            routes.MapRoute(
                "Default",
                "{controller}/{action}",
                new { controller = "QlikView", action = "SignIn" });
        }
    }
}