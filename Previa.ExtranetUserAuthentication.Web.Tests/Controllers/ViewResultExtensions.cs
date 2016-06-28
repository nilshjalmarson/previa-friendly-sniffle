using System.Web.Mvc;
using NUnit.Framework;

namespace Previa.ExtranetUserAuthentication.Web.Tests.Controllers
{
    public static class ViewResultExtensions
    {
        public static ViewResult WithInvalidModelState(this ViewResult viewResult)
        {
            Assert.That(viewResult.ViewData.ModelState.IsValid, Is.False);
            return viewResult;
        }
    }
}