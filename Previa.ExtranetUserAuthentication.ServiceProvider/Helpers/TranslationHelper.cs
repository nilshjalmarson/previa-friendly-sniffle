using System.Resources;
using log4net;

namespace Previa.ExtranetUserAuthentication.ServiceProvider.Helpers
{
    public static class TranslationHelper
    {
        private static readonly ResourceManager ExtranetResources = new ResourceManager(typeof(Resources.AppResources));
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string Translate(string resourceKey, params object[] args)
        {
            var resource = ExtranetResources.GetString(resourceKey);
            if (string.IsNullOrEmpty(resource))
            {
                resource = resourceKey;
                Log.WarnFormat("Failed to get the translation for key '{0}'.", resourceKey);
            }
            return args != null && args.Length > 0 ? string.Format(resource, args) : resource;
        }
    }
}