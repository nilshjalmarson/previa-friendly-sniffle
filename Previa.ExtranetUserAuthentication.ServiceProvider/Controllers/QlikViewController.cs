using System;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Previa.AuthenticationAuthorization.Models;
using Previa.Common.Rest;
using Previa.ExtranetUserAuthentication.ServiceProvider.Helpers;
using Previa.ExtranetUserAuthentication.ServiceProvider.Models;
using log4net;
using System.Web.Security;
using Previa.ExtranetUserAuthentication.ServiceProvider.Filters;
using System.Security.Claims;
using System.Linq;

namespace Previa.ExtranetUserAuthentication.ServiceProvider.Controllers
{
    public class QlikViewController : Controller
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string TicketAuthenticationFailedMessageId = "ticketAuthorizationFailed";
        private const string SignedOutMessageId = "signedOut";
        private const string TimedOutMessageId = "timedOut";
        private const string IdentityMappingFailedMessageId = "identityMappingFailed";

        protected readonly IExtranetUserAuthentication Authenticator;
        protected readonly IQlikViewTicket QlikViewTicket;
        protected readonly IExtranetIdentityMappingProvider IdentityMapper;


        public QlikViewController(IExtranetUserAuthentication authenticator, IQlikViewTicket qlikViewTicket, IExtranetIdentityMappingProvider identityMapper)
        {
            if (authenticator == null)
            {
                throw new ArgumentNullException("authenticator");
            }
            if (qlikViewTicket == null)
            {
                throw new ArgumentNullException("qlikViewTicket");
            }
            if (identityMapper == null)
            {
                throw new ArgumentNullException("identityMapper");
            }

            Authenticator = authenticator;
            QlikViewTicket = qlikViewTicket;
            IdentityMapper = identityMapper;
        }

        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult Secure()
        {
            var identity = System.Web.HttpContext.Current.User.Identity as ClaimsIdentity;
            var samAccountName = identity.Claims.Single(c => c.Type == "samAccountName").Value;
            return PerformQlikviewTicketing(samAccountName);
        }
                
        [HttpGet]
        [NoCache]
        public ActionResult SignOut()
        {
            try
            {
                string userId = User.Identity.Name;
                Log.DebugFormat("Signing out user {0}...", userId );
            }
            catch (Exception e)
            {
                Log.Error("Signing out a user...", e);
            }

            FormsAuthentication.SignOut();
            try
            {
                // SignOut from IDP
            }
            catch (Exception e)
            {
                Log.Error("Error while signing out...", e);
            }
            return View();
        }



        private ActionResult PerformQlikviewTicketing(string samAccountName)
        {
            // Get WebTicket from QlikView
            string webTicket;
            try
            {
                webTicket = QlikViewTicket.GetWebTicket(samAccountName);
                if (string.IsNullOrEmpty(webTicket))
                {
                    throw new InvalidOperationException("Got an empty WebTicket.");
                }
            }
            catch (Exception ex)
            {
                if (ex is RestException)
                {
                    Log.ErrorFormat("Error while fetching WebTicket from QlikView for user '{0}'. Error message was '{1}'. QlikView responded with status '{2}' and content '{3}'.",
                                    samAccountName,
                                    ex.Message,
                                    ((RestException)ex).StatusCode,
                                    ((RestException)ex).Content);
                }
                else
                {
                    Log.Error(ex.Message, ex);
                }

                ModelState.AddModelError(string.Empty, TranslationHelper.Translate("AuthenticationFailedOnStatistics"));
                CredentialsViewModel credentials = new CredentialsViewModel { Username = samAccountName };
                return View("SignIn", credentials);
            }

            // Set the authentication cookie
            FormsAuthentication.SetAuthCookie(samAccountName, false);
            string returnUrl = FormsAuthentication.GetRedirectUrl(samAccountName, false);

            // Redirect to QlikView with WebTicket
            string ticketUrl = GetQlikViewTicketUrl(webTicket, returnUrl);
            Log.Debug(string.Format("Ticket OK for user: {0}, redirecting to: {1}", samAccountName, ticketUrl));
            return Redirect(ticketUrl);
        }


        private string GetQlikViewTicketUrl(string ticket, string returnUrl)
        {
            // Parameters of Authenticate.aspx:
            //   webticket: the user's ticket
            //   try:       the url where the user will be redirected on successful authentication
            //   back:      the url where the user will be redirected on failed authentication
            return string.Format(
                "/QvAJAXZfc/Authenticate.aspx?type=html&webticket={0}&try={1}&back={2}", 
                ticket, 
                returnUrl,
                Url.Action("SignIn", new { messageId = TicketAuthenticationFailedMessageId }));
        }

        private void TrySetMessage(string messageId)
        {
            // Well this could be implemented in a prettier way. You do it!
            string infoMessage = null;
            string errorMessage = null;

            if (!string.IsNullOrEmpty(messageId))
            {
                if (messageId == TicketAuthenticationFailedMessageId)
                {
                    errorMessage = TranslationHelper.Translate("QlikViewWebTicketAuthenticationFailed");
                }
                if (messageId == SignedOutMessageId)
                {
                    infoMessage = TranslationHelper.Translate("SignedOutMessage");
                }
                if (messageId == TimedOutMessageId)
                {
                    infoMessage = TranslationHelper.Translate("TimedOutMessage");
                }
                if (messageId == IdentityMappingFailedMessageId)
                {
                    infoMessage = TranslationHelper.Translate("IdentityMappingFailed");
                }
            }

            ViewBag.InfoMessage = infoMessage;
            ViewBag.ErrorMessage = errorMessage;
        }
    }
}
