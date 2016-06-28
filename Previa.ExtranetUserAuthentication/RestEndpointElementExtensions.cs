using System;
using Previa.Common.Rest;

namespace Previa.ExtranetUserAuthentication
{
    public static class RestEndpointElementExtensions
    {
        public static IExtranetUserAuthenticationConfiguration ToExtranetUserAuthenticationConfiguration(this RestEndpointElement restEndpoint)
        {
            if (restEndpoint == null)
            {
                throw new ArgumentNullException("restEndpoint");
            }

            var config = new ExtranetUserAuthenticationConfiguration
            {
                Username = restEndpoint.Username,
                Password = restEndpoint.Password,
                BaseUrl = restEndpoint.ServiceBaseUri,
                ProxyHost = restEndpoint.ProxyHost,
                ProxyPort = string.IsNullOrEmpty(restEndpoint.ProxyHost) ? (int?)null : restEndpoint.ProxyPort
            };

            return config;
        }

        public static IQlikViewTicketConfiguration ToQlikViewTicketConfigurationConfiguration(this RestEndpointElement restEndpoint)
        {
            if (restEndpoint == null)
            {
                throw new ArgumentNullException("restEndpoint");
            }

            var config = new QlikViewTicketConfiguration
            {
                Username = restEndpoint.Username,
                Password = restEndpoint.Password,
                BaseUrl = restEndpoint.ServiceBaseUri,
                ProxyHost = restEndpoint.ProxyHost,
                ProxyPort = string.IsNullOrEmpty(restEndpoint.ProxyHost) ? (int?)null : restEndpoint.ProxyPort
            };

            return config;
        }

        public static IExtranetIdentityMappingServiceConfiguration ToExtranetIdentityMappingServiceConfiguration(this RestEndpointElement restEndpoint)
        {
            if (restEndpoint == null)
            {
                throw new ArgumentNullException("restEndpoint");
            }

            var config = new ExtranetIdentityMappingServiceConfiguration
            {
                Username = restEndpoint.Username,
                Password = restEndpoint.Password,
                BaseUrl = restEndpoint.ServiceBaseUri,
                ProxyHost = restEndpoint.ProxyHost,
                ProxyPort = string.IsNullOrEmpty(restEndpoint.ProxyHost) ? (int?)null : restEndpoint.ProxyPort
            };

            return config;
        }

    }
}