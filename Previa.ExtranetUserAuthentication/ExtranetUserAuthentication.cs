using System;
using System.Net;
using Previa.AuthenticationAuthorization.Models;
using RestSharp;
using RestSharp.Deserializers;
using Previa.Common;
using Previa.Common.Rest;
using RestSharp.Serializers;

namespace Previa.ExtranetUserAuthentication
{
    public class ExtranetUserAuthentication : IExtranetUserAuthentication
    {
        private readonly IExtranetUserAuthenticationConfiguration _configuration;

        protected IExtranetUserAuthenticationConfiguration Configuration { get { return _configuration; } }

        public ExtranetUserAuthentication(IExtranetUserAuthenticationConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }
            if (configuration.BaseUrl.IsNullOrEmpty())
            {
                throw new ArgumentException("The IExtranetUserAuthenticationConfiguration.BaseUrl property must have a value.");
            }
            _configuration = configuration;
        }

        protected ExtranetUserAuthentication()
        {
        }

        protected virtual IRestClient GetRestClient()
        {
            var restClient = new RestClient(Configuration.BaseUrl);
            if (!Configuration.Username.IsNullOrEmpty())
            {
                restClient.Authenticator = new HttpBasicAuthenticator(Configuration.Username, Configuration.Password);
            }
            if (!Configuration.ProxyHost.IsNullOrEmpty())
            {
                restClient.Proxy = Configuration.ProxyPort.HasValue ?
                                   new WebProxy(Configuration.ProxyHost, Configuration.ProxyPort.Value) :
                                   new WebProxy(Configuration.ProxyHost);
            }
            restClient.ClearHandlers();
            restClient.AddHandler("application/xml", new DotNetXmlDeserializer());
            restClient.AddHandler("*", new DotNetXmlDeserializer());
            return restClient;
        }
        
        public AuthenticationResult Authenticate(string username, string password)
        {
            if (username.IsNullOrEmpty())
            {
                throw new ArgumentNullException("username");
            }
            var credentials = new AuthenticationCredentials
            {
                username = username,
                credentials = password,
                credentialType = CredentialsType.password
            };
            return Authenticate(credentials);
        }

        public AuthenticationResult Authenticate(AuthenticationCredentials credentials)
        {
            if (credentials == null)
            {
                throw new ArgumentNullException("credentials");
            }
            if (credentials.username.IsNullOrEmpty())
            {
                throw new ArgumentException("The username property of the credentials argument must have a value.");
            }
            if (credentials.credentialType != CredentialsType.password)
            {
                throw new ArgumentException("Password is the only supported credential type.");
            }
            return AuthenticateCore(credentials);
        }

        protected virtual AuthenticationResult AuthenticateCore(AuthenticationCredentials credentials)
        {
            var request = new RestRequest("authenticate", Method.POST);
            request.RequestFormat = DataFormat.Xml;
            request.XmlSerializer = new DotNetXmlSerializer();
            request.AddBody(credentials);
            var client = GetRestClient();
            var response = client.Execute<AuthenticationResult>(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new RestException(response.ErrorMessage, response.StatusCode, response.Content, response.ErrorException);
            }
            return response.Data;
        }
    }
}