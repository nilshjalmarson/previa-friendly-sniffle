using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Previa.ExtranetUserAuthentication
{
    public class QlikViewTicketConfiguration : IQlikViewTicketConfiguration
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string BaseUrl { get; set; }
        public string ProxyHost { get; set; }
        public int? ProxyPort { get; set; }
    }
}
