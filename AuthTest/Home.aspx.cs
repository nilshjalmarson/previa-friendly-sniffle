using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AuthTest
{
    public partial class Home : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            usrName.Text = "Logged in as: " + ((HttpContext.Current.User.Identity.Name != null) && (HttpContext.Current.User.Identity.Name.Length == 0) ? "Anonymous user" : HttpContext.Current.User.Identity.Name);
        }
    }
}