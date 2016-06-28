namespace Previa.ExtranetUserAuthentication
{
    public interface IExtranetUserAuthenticationConfiguration
    {
        string Username { get; }
        string Password { get; }
        string BaseUrl { get; }
        string ProxyHost { get; }
        int? ProxyPort { get; }
    }
}