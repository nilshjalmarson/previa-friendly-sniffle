using Autofac.Integration.Mvc;
using Bootstrap.Extensions.StartupTasks;
using System.Web.Mvc;
using Bootstrap;
using Autofac;
namespace Previa.ExtranetUserAuthentication.ServiceProvider.Bootstrapping
{
    public class DependencyResolverStartupTask : IStartupTask
    {
        private IDependencyResolver _originalResolver;

        public void Reset()
        {
            if (_originalResolver != null)
            {
                DependencyResolver.SetResolver(_originalResolver);
            }
        }

        public void Run()
        {
            _originalResolver = DependencyResolver.Current;
            DependencyResolver.SetResolver(new AutofacDependencyResolver((IContainer)Bootstrapper.Container));
        }
    }
}