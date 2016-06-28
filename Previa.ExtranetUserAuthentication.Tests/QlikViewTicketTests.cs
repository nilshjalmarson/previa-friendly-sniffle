using System;
using System.Net;
using System.Xml.Linq;
using NUnit.Framework;
using Previa.Common.Rest;
using RestSharp;
using RestSharp.Deserializers;
using Rhino.Mocks;
using Previa.AuthenticationAuthorization.Models;
using RestSharp.Serializers;

namespace Previa.ExtranetUserAuthentication.Tests
{
    [TestFixture]
    public class QlikViewTicketTests
    {
        [Test]
        public void ConstructorThrowsOnNullArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new QlikViewTicket(null));
        }

        [Test]
        public void ConstructorThrowsOnNullBaseUrl()
        {
            var config = MockRepository.GenerateStub<IQlikViewTicketConfiguration>();

            Assert.Throws<ArgumentException>(() => new QlikViewTicket(config));
        }

        [Test]
        public void ConstructorSetsProperties()
        {
            var config = MockRepository.GenerateStub<IQlikViewTicketConfiguration>();
            config.Stub(c => c.BaseUrl).Return("http://example.com");

            var qlikView = new TestQlikViewTicket(config);

            Assert.That(qlikView.ConfigurationForTesting, Is.Not.Null);
        }

        [Test]
        public void GetRestClientReturnsSetsCorrectBaseUrl()
        {
            var config = MockRepository.GenerateStub<IQlikViewTicketConfiguration>();
            config.Stub(c => c.BaseUrl).Return("http://example.com");
            var qlikView = new TestQlikViewTicket(config);

            var restClient = qlikView.GetRestClientForTesting();

            Assert.That(restClient.BaseUrl, Is.EqualTo(config.BaseUrl));
        }

        [Test]
        public void GetRestClientCanEnableBasicAuthentication()
        {
            var config = MockRepository.GenerateStub<IQlikViewTicketConfiguration>();
            config.Stub(c => c.BaseUrl).Return("http://example.com");
            config.Stub(c => c.Username).Return("username");
            config.Stub(c => c.Password).Return("password");
            var qlikView = new TestQlikViewTicket(config);

            var restClient = qlikView.GetRestClientForTesting();

            Assert.That(restClient.Authenticator, Is.InstanceOf<HttpBasicAuthenticator>());
        }

        [Test]
        public void GetRestClientCanEnableProxy()
        {
            var config = MockRepository.GenerateStub<IQlikViewTicketConfiguration>();
            config.Stub(c => c.BaseUrl).Return("http://example.com");
            config.Stub(c => c.ProxyHost).Return("http://example.com/proxy");
            config.Stub(c => c.ProxyPort).Return(1337);
            var qlikView = new TestQlikViewTicket(config);

            var restClient = qlikView.GetRestClientForTesting();

            Assert.That(restClient.Proxy, Is.Not.Null);
        }

        [Test]
        public void GetWebTicketThrowsOnEmptyUsername()
        {
            var qlikView = new TestQlikViewTicket();

            Assert.Throws<ArgumentNullException>(() => qlikView.GetWebTicket(null));
            Assert.Throws<ArgumentNullException>(() => qlikView.GetWebTicket(string.Empty));
        }

        [Test]
        public void GetWebTicketCallsExecuteWithCorrectRequest()
        {
            var username = "username";
            var xml = new XElement("Global",
	            new XAttribute("method", "GetWebTicket"),
	            new XElement("UserId", username.Trim()));
            var serializedUsername = xml.ToString();
            var restClient = MockRepository.GenerateMock<IRestClient>();
            restClient.Expect(rc =>
                rc.Execute(Arg<RestRequest>.Matches(r =>
                    r.Resource == "QvAJAXZfc/GetWebTicket.aspx" &&
                    r.Method == Method.POST &&
                    r.HasBody(serializedUsername))))
                .Return(null);
            var qlikView = new TestQlikViewTicket(restClient);

            try
            {
                qlikView.GetWebTicket(username);
            }
            catch { }

            restClient.VerifyAllExpectations();
        }

        [Test]
        public void GetWebTicketReturnsResultFromRestResponse()
        {
            var ticket = "ticket";
            var serializedTicket = string.Format("<Global><_retval_>{0}</_retval_></Global>", ticket);
            var response = new RestResponse() { Content = serializedTicket, StatusCode = HttpStatusCode.OK };
            var restClient = MockRepository.GenerateStub<IRestClient>();
            restClient.Stub(rc => rc.Execute(Arg<IRestRequest>.Is.Anything)).Return(response);
            var qlikView = new TestQlikViewTicket(restClient);

            var actual = qlikView.GetWebTicket("username");

            Assert.That(actual, Is.EqualTo(ticket));
        }

        [TestCase(HttpStatusCode.BadGateway)]
        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.Forbidden)]
        [TestCase(HttpStatusCode.InternalServerError)]
        [TestCase(HttpStatusCode.NoContent)]
        [TestCase(HttpStatusCode.NotFound)]
        [TestCase(HttpStatusCode.NotImplemented)]
        [TestCase(HttpStatusCode.ServiceUnavailable)]
        public void GetWebTicketThrowsRestExceptionWhenResponseStatusIsNotOk(HttpStatusCode statusCode)
        {
            var response = new RestResponse { StatusCode = statusCode };
            var restClient = MockRepository.GenerateStub<IRestClient>();
            restClient.Stub(rc => rc.Execute(Arg<IRestRequest>.Is.Anything)).Return(response);
            var qlikView = new TestQlikViewTicket(restClient);

            Assert.Throws<RestException>(() => qlikView.GetWebTicket("username"));
        }

        private class TestQlikViewTicket : QlikViewTicket
        {
            private readonly IRestClient _restClient;
            //private readonly Func<AuthenticationCredentials, AuthenticationResult> _authenticateCore;

            public IQlikViewTicketConfiguration ConfigurationForTesting
            {
                get { return base.Configuration; }
            }

            public TestQlikViewTicket(IQlikViewTicketConfiguration configuration)
                : base(configuration)
            {
            }

            public TestQlikViewTicket(IRestClient restClient)
            {
                _restClient = restClient;
            }

            //public TestQlikViewTicket(Func<AuthenticationCredentials, AuthenticationResult> authenticate)
            //{
            //    if (authenticate != null)
            //    {
            //        _authenticateCore = authenticate;
            //    }
            //}

            public TestQlikViewTicket()
            {
            }

            protected override IRestClient GetRestClient()
            {
                return _restClient ?? base.GetRestClient();
            }

            public IRestClient GetRestClientForTesting()
            {
                return GetRestClient();
            }

            //protected override AuthenticationResult AuthenticateCore(AuthenticationCredentials credentials)
            //{
            //    if (_authenticateCore != null)
            //    {
            //        return _authenticateCore(credentials);
            //    }
            //    return base.AuthenticateCore(credentials);
            //}
        }
    }
}