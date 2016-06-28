namespace Previa.ExtranetUserAuthentication
{
    public class ExtranetUserAuthenticationConfiguration : IExtranetUserAuthenticationConfiguration
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string BaseUrl { get; set; }
        public string ProxyHost { get; set; }
        public int? ProxyPort { get; set; }
    }
}