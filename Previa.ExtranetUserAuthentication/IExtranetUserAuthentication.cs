using Previa.AuthenticationAuthorization.Models;
namespace Previa.ExtranetUserAuthentication
{
    public interface IExtranetUserAuthentication
    {
        AuthenticationResult Authenticate(string username, string password);
        AuthenticationResult Authenticate(AuthenticationCredentials credentials);
    }
}