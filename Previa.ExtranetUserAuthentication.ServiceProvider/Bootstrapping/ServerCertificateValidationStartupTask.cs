using System.Net;
using Bootstrap.Extensions.StartupTasks;
using System.Net.Security;
using Previa.Common.Rest;

namespace Previa.ExtranetUserAuthentication.ServiceProvider.Bootstrapping
{
    public class ServerCertificateValidationStartupTask : IStartupTask
    {
        private RemoteCertificateValidationCallback _originalValidator;

        public void Reset()
        {
            if (_originalValidator != null)
            {
                ServicePointManager.ServerCertificateValidationCallback = _originalValidator;
            }
        }

        public void Run()
        {
            _originalValidator = ServicePointManager.ServerCertificateValidationCallback;
            ServicePointManager.ServerCertificateValidationCallback = ServerCertificateValidator.ValidateServerCertificate;
        }
    }
}