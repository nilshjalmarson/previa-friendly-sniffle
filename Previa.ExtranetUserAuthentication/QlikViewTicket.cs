using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Serializers;
using System.Xml.Linq;
using System.Net;
using System.IO;
using Previa.Common;
using Previa.Common.Rest;
using log4net;

namespace Previa.ExtranetUserAuthentication
{
    public class QlikViewTicket : IQlikViewTicket
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IQlikViewTicketConfiguration _configuration;

        protected IQlikViewTicketConfiguration Configuration { get { return _configuration; } }

        public QlikViewTicket(IQlikViewTicketConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }
            if (configuration.BaseUrl.IsNullOrEmpty())
            {
                throw new ArgumentException("The IQlikViewTicketConfiguration.BaseUrl property must have a value.");
            }
            _configuration = configuration;
        }

        protected QlikViewTicket()
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
            restClient.AddHandler("application/xml", new XmlDeserializer());
            restClient.AddHandler("*", new XmlDeserializer());
            return restClient;
        }

        public string GetWebTicket(string username)
        {
            //debug
            //return "3nLnEf0wK5OUIAqZeJj0iuZm0ssExKk57I6k5grM";

            if (username.IsNullOrEmpty())
            {
                throw new ArgumentNullException("username");
            }

            var xml = new XElement("Global",
	            new XAttribute("method", "GetWebTicket"),
	            new XElement("UserId", username.Trim()));

            var request = new RestRequest("QvAJAXZfc/GetWebTicket.aspx", Method.POST);
            request.AddParameter("ignored", xml.ToString(), ParameterType.RequestBody);
            var client = GetRestClient();

            Log.DebugFormat("Requesting WebTicket from QlikView. Posting '{0}' to '{1}'.", xml, client.BuildUri(request));

            RestResponse response;
            try
            {
                response = client.Execute(request);
            } 
            catch (Exception ex)
            {
                Log.Error("Error when executing REST request for getting a WebTicket from QlikView.", ex);
                throw;
            }
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new RestException(response.ErrorMessage, response.StatusCode, response.Content, response.ErrorException);
            }
            if (response.Content.IsNullOrEmpty())
            {
                throw new RestException("QlikView sent an empty response.", response.StatusCode, response.Content);
            }
            XElement responseXml = null;
            try
            {
                responseXml = XElement.Parse(response.Content);
            } 
            catch
            {
                Log.ErrorFormat("Invalid GetTicket response content received from QlikView: '{0}'", response.Content);
                throw new FormatException("Could not parse the response that QlikView sent into XML.");
            }
            var ticketElement = responseXml.Descendants("_retval_").FirstOrDefault();
            if (ticketElement == null)
            {
                Log.ErrorFormat("Invalid GetTicket response content received from QlikView: '{0}'", responseXml);
                throw new FormatException("The XML in the response that QlikView sent did not follow the expected structure.");
            }
            return ticketElement.Value;
        }
    }
}
