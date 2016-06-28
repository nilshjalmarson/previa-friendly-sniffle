using System.Web.Mvc;
using MvcContrib.TestHelper;
using NUnit.Framework;
using System.Web.Routing;
using Previa.ExtranetUserAuthentication.Web.Bootstrapping;
using Previa.ExtranetUserAuthentication.Web.Controllers;

namespace Previa.ExtranetUserAuthentication.Web.Tests.Routing
{
    [TestFixture]
    public class RoutingStartupTaskTests
    {
        [TestFixtureSetUp]
        public void BeforeFixture()
        {
            var routingTask = new RoutingStartupTask();
            routingTask.Run();
        }

        [Test]
        public void DefaultRouteLeadsToQlikViewSignIn()
        {
            "~/".WithMethod(HttpVerbs.Get)
                .ShouldMapTo<QlikViewController>(action => action.SignIn((string)null));
        }
    }
}