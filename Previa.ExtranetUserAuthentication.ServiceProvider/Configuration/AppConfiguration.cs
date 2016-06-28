using System;
using Previa.Common.Configuration;
using Previa.Common.Rest;
namespace Previa.ExtranetUserAuthentication.ServiceProvider.Configuration
{
    public class AppConfiguration
    {
        private static readonly Lazy<AppConfiguration> LazyInstance = new Lazy<AppConfiguration>(() => new AppConfiguration());

        public static AppConfiguration Instance { get { return LazyInstance.Value; } }

        public RestConfiguration Rest
        {
            get { return ConfigurationBase.GetInstance<RestConfiguration>("rest"); }
        }
    }
}