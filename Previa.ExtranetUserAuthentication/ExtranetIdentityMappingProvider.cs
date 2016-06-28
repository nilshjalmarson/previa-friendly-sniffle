using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Previa.Common;
using RestSharp;
using System.Net;
using RestSharp.Deserializers;
using Previa.ExtranetIdentityMapping.Models;
using RestSharp.Serializers;
using Previa.Common.Rest;

namespace Previa.ExtranetUserAuthentication
{
    public class ExtranetIdentityMappingProvider : IExtranetIdentityMappingProvider
    {
        private readonly IExtranetIdentityMappingServiceConfiguration _configuration;

        protected IExtranetIdentityMappingServiceConfiguration Configuration { get { return _configuration; } }


        public ExtranetIdentityMappingProvider(IExtranetIdentityMappingServiceConfiguration configuration)
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


        protected ExtranetIdentityMappingProvider()
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


        public MappingResponse MapLocalUser(se.previa.schemas.baseobjects.v1.OrganizationRegistrationNumber orgNo, string localUserName)
        {
            if (localUserName.IsNullOrEmpty())
            {
                throw new ArgumentNullException("localUserName");
            }
            var mappingRequest = new MappingRequest
            {
                LocalUserName = localUserName,
                OrganizationRegistrationNumber = orgNo,
            };
            return MapLocalUser(mappingRequest);
        }

        public MappingResponse MapLocalUser(MappingRequest mappingRequest)
        {
            if (mappingRequest == null)
            {
                throw new ArgumentNullException("mappingRequest");
            }
            if (mappingRequest.LocalUserName.IsNullOrEmpty())
            {
                throw new ArgumentException("The local username property of the mappingrequest argument must have a value.");
            }
            return MapLocalUserCore(mappingRequest);
        }


        protected virtual MappingResponse MapLocalUserCore(MappingRequest credentials)
        {
            var request = new RestRequest("map", Method.POST);
            request.RequestFormat = DataFormat.Xml;
            request.XmlSerializer = new DotNetXmlSerializer();
            request.AddBody(credentials);
            var client = GetRestClient();
            var response = client.Execute<MappingResponse>(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new RestException(response.ErrorMessage, response.StatusCode, response.Content, response.ErrorException);
            }
            return response.Data;
        }

    }
}
