using System.Linq;
using RestSharp;

namespace Previa.ExtranetUserAuthentication.Tests
{
    public static class RestRequestExtensions
    {
        public static bool HasParameter(this IRestRequest restRequest, ParameterType type, string name, string value)
        {
            return restRequest.Parameters.Any(p =>
                                              type == p.Type &&
                                              name == p.Name &&
                                              value.Equals(p.Value));
        }

        public static bool HasBody(this IRestRequest restRequest, string serializedBody)
        {
            return restRequest.Parameters.Any(p =>
                                              p.Type == ParameterType.RequestBody &&
                                              (string)p.Value == serializedBody);
        }
    }
}