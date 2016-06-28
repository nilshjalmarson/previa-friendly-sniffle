using System;
using System.Globalization;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Previa.AuthenticationAuthorization.Models;
using Previa.Common.Rest;
using Previa.ExtranetUserAuthentication.Web.Helpers;
using Previa.ExtranetUserAuthentication.Web.Models;
using log4net;
using System.Web.Security;
using Previa.ExtranetUserAuthentication.Web.Filters;
using Previa.ExtranetIdentityMapping.Models;
using se.previa.schemas.baseobjects.v1;

namespace Previa.ExtranetUserAuthentication.Web.Controllers
{
    //[RequireHttps]
    public class QlikViewController : Controller
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string TicketAuthenticationFailedMessageId = "ticketAuthorizationFailed";
        private const string SignedOutMessageId = "signedOut";
        private const string TimedOutMessageId = "timedOut";
        private const string IdentityMappingFailedMessageId = "identityMappingFailed";

        private const string SSOCookieName = "PreviaLocalSSO";

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

        /// <summary>
        /// TODO - DEBUG AND SHOULD BE REMOVED
        /// </summary>
        /// <returns></returns>
        //[HttpGet]
        //[NoCache]
        //public ActionResult TestLogin()
        //{
        //    Log.Warn("DEBUGGING LOGIN WITH PRESET USER");
        //    AuthenticationResult autoAuthenticated = new AuthenticationResult() { username = "johsno", status = AuthenticationStatus.valid };
        //    return PerformQlikviewTicketing(autoAuthenticated);
        //}


        private void createSSOCookie(string orgNr, string localUserName, string previaUserName)
        {
            var localCookie = new HttpCookie(SSOCookieName);
            localCookie.Values.Add("OrgNr", orgNr);
            localCookie.Values.Add("LocalUserName", localUserName);
            localCookie.Values.Add("PreviaUserName", previaUserName);
            localCookie.Values.Add("LoggedInTime", DateTime.Now.ToString(CultureInfo.InvariantCulture));
            int expirationMinutes = 12 * 60;
            var expireTime = System.Configuration.ConfigurationManager.AppSettings["SSO_CookieExpirationMinutes"];
            if (!string.IsNullOrEmpty(expireTime))
            {
                int newTime;
                if (int.TryParse(expireTime, out newTime))
                    expirationMinutes = newTime;
            }
            localCookie.Expires = DateTime.Now.AddMinutes(expirationMinutes);
            Response.Cookies.Add(localCookie);

        }

        /// <summary>
        /// Special for Arbetsförmedlingen that uses SAML authentication
        /// Should be secured by Heimore.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [NoCache]
        [AuthorizeIPAddress]
        public ActionResult LocalUser()
        {
            try
            {
                string referrer = Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : "MISSING REFERRER";
                Log.Info(string.Format("LocalUser call - referrer: '{0}', userHostName:'{1}', userHostAdress:'{2}' ", referrer, Request.UserHostName, Request.UserHostAddress ));
            }
            catch (Exception e)
            {
                Log.Error("LOG LOCALUSER", e);
            }

            MappingRequest mappingRequest = new MappingRequest();

            //mappingRequest.LocalUserName = "Testmia";
            //mappingRequest.OrganizationRegistrationNumber = OrganizationRegistrationNumber.Parse("SE_121212-1212");
            try
            {
                //Here we should get local username and orgno from a header that is set by Heimore..
                mappingRequest.LocalUserName = Request.Headers["userid"];
                mappingRequest.OrganizationRegistrationNumber = OrganizationRegistrationNumber.Parse(Request.Headers["orgnr"]);
            }
            catch (Exception e)
            {
                Log.Error("LocalUser failed to get headers", e);
                return RedirectToAction("SignIn", new { messageId = IdentityMappingFailedMessageId });
            }

            Log.Warn(string.Format("Attempting to automatically grant local userid: '{0}' access to QlikView", mappingRequest.LocalUserName));

            try
            {
                //TODO Remove - debug cookie creation only this line
                createSSOCookie(mappingRequest.OrganizationRegistrationNumber.ToString(), mappingRequest.LocalUserName, "FAKE");

                var mappedUser = IdentityMapper.MapLocalUser(mappingRequest);
                if (mappedUser != null && !string.IsNullOrEmpty(mappedUser.ExtranetUserName))
                {
                    Log.Warn(string.Format("Local identity successfully converted from '{0}' to '{1}' and automatically granted access!", mappingRequest.LocalUserName, mappedUser.ExtranetUserName));
                    createSSOCookie(mappingRequest.OrganizationRegistrationNumber.ToString(), mappingRequest.LocalUserName, mappedUser.ExtranetUserName);

                    AuthenticationResult autoAuthenticated = new AuthenticationResult { username = mappedUser.ExtranetUserName, status = AuthenticationStatus.valid };
                    return PerformQlikviewTicketing(autoAuthenticated);
                }

                Log.Error(string.Format("Failed to map local userid: '{0}'", mappingRequest.LocalUserName));
                return RedirectToAction("SignIn", new { messageId = IdentityMappingFailedMessageId });
            }
            catch (Exception ex)
            {
                Log.Error("MapLocalUser exception", ex);
                return RedirectToAction("SignIn", new { messageId = IdentityMappingFailedMessageId });
            }
        }


        [HttpGet]
        [NoCache]
        public ActionResult SignIn(string messageId = "")
        {
            if (messageId == SignedOutMessageId && User.Identity.IsAuthenticated)
            {
                Log.WarnFormat("The user '{0}' was shown the 'signed out' message on the sign in page, but was actually not signed out.", User.Identity.Name);
            }

            if (Request.Cookies != null)
            {
                var hasSSO = Request.Cookies[SSOCookieName];
                if (hasSSO != null && hasSSO.Values["OrgNr"] != null)
                {
                    TrySetMessage(messageId);
                    string localUser = hasSSO.Values["LocalUserName"];
                    if (localUser == null)
                        localUser = "-NO-USERNAME-";

                    var urlKey = "SSO_" + hasSSO.Values["OrgNr"];
                    var redirectURL = System.Configuration.ConfigurationManager.AppSettings[urlKey];

                    string loggedInTime = hasSSO.Values["LoggedInTime"];
                    if (loggedInTime == null)
                        loggedInTime = "-TIME_MISSING-";

                    if (!string.IsNullOrEmpty(redirectURL))
                    {
                        if (string.IsNullOrEmpty(messageId))
                        {
                            Log.InfoFormat("User {0} comes from SSO, redirects to SSO url:'{1}', logged in {2}", localUser, redirectURL, loggedInTime);
                            return Redirect(redirectURL);
                        }

                        Log.InfoFormat("SignIn message: {0} - user {1} comes from SSO, redirects to SSOPage: '{2}'", messageId, localUser, redirectURL);
                        var imgPath = Url.Content("~/content/images/" + hasSSO.Values["OrgNr"] + ".png");
                        var companyName = System.Configuration.ConfigurationManager.AppSettings["CNAME_" + hasSSO.Values["OrgNr"]];

                        var ssoVM = new SSORedirectViewModel() { RedirectLink = redirectURL, CompanyImageUrl = imgPath, CompanyName = companyName };
                        return View("SSORedirect", ssoVM);
                    }

                    Log.WarnFormat("User comes from SSO but no url is configured for orgnr '{0}'", urlKey);
                }
            }
            TrySetMessage(messageId);
            return View(new CredentialsViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [NoCache]
        public ActionResult SignIn(CredentialsViewModel credentials)
        {
            if (credentials == null)
            {
                throw new ArgumentNullException("credentials");
            }
            if (!ModelState.IsValid)
            {
                return View(credentials);
            }

            AuthenticationResult authenticationResult;
            try
            {
                authenticationResult = Authenticator.Authenticate(credentials.Username, credentials.Password);
            }
            catch (RestException ex)
            {
                Log.ErrorFormat("Error while authenticating user '{0}'. Error message was '{1}'. The service responded with status '{2}' and content '{3}'.",
                    credentials.Username,
                    ex.Message,
                    ex.StatusCode,
                    ex.Content);

                ModelState.AddModelError(string.Empty, TranslationHelper.Translate("AuthenticationFailedOnService"));
                return View(credentials);
            }

            // Display error message if authentication failed
            if (authenticationResult == null ||
                authenticationResult.status != AuthenticationStatus.valid)
            {
                if (authenticationResult != null &&
                    authenticationResult.status != AuthenticationStatus.valid)
                {
                    ModelState.AddModelError(string.Empty, TranslationHelper.Translate("AuthenticationFailedUserDisabled"));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, TranslationHelper.Translate("AuthenticationFailed"));
                }
                return View(credentials);
            }

            return PerformQlikviewTicketing(authenticationResult);
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
                //if (Request.Cookies != null && Request.Cookies[SSOCookieName] != null)
                //{
                //    var localCookie = new HttpCookie(SSOCookieName);
                //    localCookie.Expires = DateTime.Now.AddDays(-1d);
                //    Response.Cookies.Add(localCookie);
                //    Log.Debug("Resetting SSO login cookie...");
                //}
                if (Request.Cookies != null)
                {
                    var hasSSOCookie = Request.Cookies[SSOCookieName];
                    if (hasSSOCookie != null && hasSSOCookie.Values["OrgNr"] != null)
                    {
                        TrySetMessage(SignedOutMessageId);
                        string localUser = hasSSOCookie.Values["LocalUserName"];
                        if (localUser == null)
                            localUser = "-NO-USERNAME-";
                        string loggedInTime = hasSSOCookie.Values["LoggedInTime"];
                        if (loggedInTime == null)
                            loggedInTime = "-TIME_MISSING-";

                        var urlKey = "SSO_" + hasSSOCookie.Values["OrgNr"];
                        var redirectURL = System.Configuration.ConfigurationManager.AppSettings[urlKey];
                        if (!string.IsNullOrEmpty(redirectURL))
                        {
                            Log.InfoFormat("User {0} comes from SSO, redirects to Info Page:'{1}', cookie logged in '{2}'", localUser, redirectURL, loggedInTime);
                            var imgPath = Url.Content("~/content/images/" + hasSSOCookie.Values["OrgNr"] + ".png");
                            var companyName = System.Configuration.ConfigurationManager.AppSettings["CNAME_" + hasSSOCookie.Values["OrgNr"]];

                            var ssoVM = new SSORedirectViewModel { RedirectLink = redirectURL, CompanyImageUrl = imgPath, CompanyName = companyName };
                            return View("SSORedirect", ssoVM);
                        }

                        Log.WarnFormat("User {0} comes from SSO but no url is configured for orgnr '{1}'", localUser, urlKey);
                    }
                }

            }
            catch (Exception e)
            {
                Log.Error("Error while signing out...", e);
            }
            return RedirectToAction("SignIn", new { messageId = SignedOutMessageId });
        }



        private ActionResult PerformQlikviewTicketing(AuthenticationResult authenticationResult)
        {
            //TODO MAKE SURE THESE 4 Debug lines are commented out, use only if no connection to WebTicket..
            //FormsAuthentication.SetAuthCookie(authenticationResult.username, false);
            //string debugUrl = FormsAuthentication.GetRedirectUrl(authenticationResult.username, false);
            //debugUrl = GetQlikViewTicketUrl("TEST", debugUrl);
            //return Redirect(debugUrl);


            // Get WebTicket from QlikView
            string webTicket;
            try
            {
                webTicket = QlikViewTicket.GetWebTicket(authenticationResult.username);
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
                                    authenticationResult.username,
                                    ex.Message,
                                    ((RestException)ex).StatusCode,
                                    ((RestException)ex).Content);
                }
                else
                {
                    Log.Error(ex.Message, ex);
                }

                ModelState.AddModelError(string.Empty, TranslationHelper.Translate("AuthenticationFailedOnStatistics"));
                CredentialsViewModel credentials = new CredentialsViewModel { Username = authenticationResult.username };
                return View("SignIn", credentials);
            }

            // Set the authentication cookie
            FormsAuthentication.SetAuthCookie(authenticationResult.username, false);
            string returnUrl = FormsAuthentication.GetRedirectUrl(authenticationResult.username, false);

            // Redirect to QlikView with WebTicket
            string ticketUrl = GetQlikViewTicketUrl(webTicket, returnUrl);
            Log.Debug(string.Format("Ticket OK for user: {0}, redirecting to: {1}", authenticationResult.username, ticketUrl));
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
