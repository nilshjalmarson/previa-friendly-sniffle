using System;
using System.Reflection;
using Autofac;
using Autofac.Integration.Mvc;
using Bootstrap.Autofac;
using Previa.ExtranetUserAuthentication.ServiceProvider.Configuration;
using log4net;

namespace Previa.ExtranetUserAuthentication.ServiceProvider.Bootstrapping
{
    public class AutofacRegistration : IAutofacRegistration
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Register(ContainerBuilder containerBuilder)
        {
            Log.Debug("Building Autofac container...");

            // Register custom types
            containerBuilder
                .Register(context => AppConfiguration.Instance.Rest.GetEndpoint("extranetUserAuthentication").ToExtranetUserAuthenticationConfiguration())
                .As<IExtranetUserAuthenticationConfiguration>();

            containerBuilder
                .Register(context => AppConfiguration.Instance.Rest.GetEndpoint("qlikViewTicketServer").ToQlikViewTicketConfigurationConfiguration())
                .As<IQlikViewTicketConfiguration>();

            containerBuilder
                .Register(context => AppConfiguration.Instance.Rest.GetEndpoint("extranetIdentityMapping").ToExtranetIdentityMappingServiceConfiguration())
                .As<IExtranetIdentityMappingServiceConfiguration>();

            containerBuilder
                .RegisterType<QlikViewTicket>()
                .As<IQlikViewTicket>();

            containerBuilder
                .RegisterType<ExtranetUserAuthentication>()
                .As<IExtranetUserAuthentication>();

            containerBuilder
                .RegisterType<ExtranetIdentityMappingProvider>()
                .As<IExtranetIdentityMappingProvider>();

            // Register ASP.NET-related types
            containerBuilder.RegisterControllers(typeof(MvcApplication).Assembly);
            containerBuilder.RegisterModelBinders(Assembly.GetExecutingAssembly());
            containerBuilder.RegisterModule(new AutofacWebTypesModule());
            containerBuilder.RegisterModelBinderProvider();


        }
    }
}