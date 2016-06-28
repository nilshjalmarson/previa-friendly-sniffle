using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Previa.ExtranetUserAuthentication
{
    public interface IQlikViewTicketConfiguration
    {
        string Username { get; }
        string Password { get; }
        string BaseUrl { get; }
        string ProxyHost { get; }
        int? ProxyPort { get; }
    }
}