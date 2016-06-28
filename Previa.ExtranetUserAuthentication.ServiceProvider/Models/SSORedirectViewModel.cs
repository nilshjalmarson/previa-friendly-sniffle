using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Previa.ExtranetUserAuthentication.ServiceProvider.Models
{
    public class SSORedirectViewModel
    {
        public string CompanyName { get; set; }

        public string CompanyImageUrl { get; set; }

        public string RedirectLink { get; set; }
    }
}