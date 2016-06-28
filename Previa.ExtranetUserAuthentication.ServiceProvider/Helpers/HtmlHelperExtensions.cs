using System.Web.Mvc;
namespace Previa.ExtranetUserAuthentication.ServiceProvider.Helpers
{
    public static class HtmlHelperExtensions
    {
        public static string Translate(this HtmlHelper helper, string resourceKey, params object[] args)
        {
            return TranslationHelper.Translate(resourceKey, args);
        }
    }
}