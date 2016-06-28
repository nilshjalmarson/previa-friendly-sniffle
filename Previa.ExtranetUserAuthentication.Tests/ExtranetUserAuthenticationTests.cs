using System;
using System.Net;
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
    public class ExtranetUserAuthenticationTests
    {
        [Test]
        public void ConstructorThrowsOnNullArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new ExtranetUserAuthentication(null));
        }

        [Test]
        public void ConstructorThrowsOnNullBaseUrl()
        {
            var config = MockRepository.GenerateStub<IExtranetUserAuthenticationConfiguration>();

            Assert.Throws<ArgumentException>(() => new ExtranetUserAuthentication(config));
        }

        [Test]
        public void ConstructorSetsProperties()
        {
            var config = MockRepository.GenerateStub<IExtranetUserAuthenticationConfiguration>();
            config.Stub(c => c.BaseUrl).Return("http://example.com");

            var personRegistry = new TestExtranetUserAuthentication(config);

            Assert.That(personRegistry.ConfigurationForTesting, Is.Not.Null);
        }

        [Test]
        public void GetRestClientReturnsSetsCorrectBaseUrl()
        {
            var config = MockRepository.GenerateStub<IExtranetUserAuthenticationConfiguration>();
            config.Stub(c => c.BaseUrl).Return("http://example.com");
            var personRegistry = new TestExtranetUserAuthentication(config);

            var restClient = personRegistry.GetRestClientForTesting();

            Assert.That(restClient.BaseUrl, Is.EqualTo(config.BaseUrl));
        }

        [Test]
        public void GetRestClientCanEnableBasicAuthentication()
        {
            var config = MockRepository.GenerateStub<IExtranetUserAuthenticationConfiguration>();
            config.Stub(c => c.BaseUrl).Return("http://example.com");
            config.Stub(c => c.Username).Return("username");
            config.Stub(c => c.Password).Return("password");
            var personRegistry = new TestExtranetUserAuthentication(config);

            var restClient = personRegistry.GetRestClientForTesting();

            Assert.That(restClient.Authenticator, Is.InstanceOf<HttpBasicAuthenticator>());
        }

        [Test]
        public void GetRestClientCanEnableProxy()
        {
            var config = MockRepository.GenerateStub<IExtranetUserAuthenticationConfiguration>();
            config.Stub(c => c.BaseUrl).Return("http://example.com");
            config.Stub(c => c.ProxyHost).Return("http://example.com/proxy");
            config.Stub(c => c.ProxyPort).Return(1337);
            var personRegistry = new TestExtranetUserAuthentication(config);

            var restClient = personRegistry.GetRestClientForTesting();

            Assert.That(restClient.Proxy, Is.Not.Null);
        }

        [Test]
        public void AuthenticateThrowsOnEmptyUsername()
        {
            var extranet = new TestExtranetUserAuthentication();

            Assert.Throws<ArgumentNullException>(() => extranet.Authenticate(null, "password"));
            Assert.Throws<ArgumentNullException>(() => extranet.Authenticate(string.Empty, "password"));
        }

        [Test]
        public void AuthenticateThrowsOnNullCredentials()
        {
            var extranet = new TestExtranetUserAuthentication();

            Assert.Throws<ArgumentNullException>(() => extranet.Authenticate(null));
        }

        [Test]
        public void AuthenticateThrowsOnEmptyCredentialsUsername()
        {
            var extranet = new TestExtranetUserAuthentication();
            var credentials = new AuthenticationCredentials();

            Assert.Throws<ArgumentException>(() => extranet.Authenticate(credentials));
        }

        [Test]
        public void AuthenticateWithUsernameAndPasswordCallsAuthenticateCoreWithCorrectCredentials()
        {
            var username = "username";
            var password = "password";
            var authenticateCore = MockRepository.GenerateStub<Func<AuthenticationCredentials, AuthenticationResult>>();
            authenticateCore.Stub(a => a.Invoke(Arg<AuthenticationCredentials>.Is.Anything)).Return(null);
            var extranet = new TestExtranetUserAuthentication(authenticateCore);

            extranet.Authenticate(username, password);

            authenticateCore.AssertWasCalled(a => a.Invoke(Arg<AuthenticationCredentials>.Matches(
                credentials => credentials.username == username &&
                    credentials.credentials == password &&
                    credentials.credentialType == CredentialsType.password)));
        }

        [Test]
        public void AuthenticateWithCredentialsCallsAuthenticateCore()
        {
            var credentials = new AuthenticationCredentials
            {
                username = "username",
                credentials = "password",
                credentialType = CredentialsType.password
            };
            var authenticateCore = MockRepository.GenerateStub<Func<AuthenticationCredentials, AuthenticationResult>>();
            authenticateCore.Stub(a => a.Invoke(Arg<AuthenticationCredentials>.Is.Anything)).Return(null);
            var extranet = new TestExtranetUserAuthentication(authenticateCore);

            extranet.Authenticate(credentials);

            authenticateCore.AssertWasCalled(a => a.Invoke(credentials));
        }

        [TestCase(CredentialsType.otp)]
        [TestCase(CredentialsType.pin)]
        [TestCase(CredentialsType.x509)]
        public void AuthenticateOnlyAcceptsPasswordAsCredential(CredentialsType credentialType)
        {
            var extranet = new TestExtranetUserAuthentication();
            var credentials = new AuthenticationCredentials
            {
                username = "username",
                credentialType = credentialType
            };

            Assert.Throws<ArgumentException>(() => extranet.Authenticate(credentials));
        }

        [Test]
        public void AuthenticateCallsExecuteWithCorrectRequest()
        {
            var credentials = new AuthenticationCredentials
            {
                username = "username",
                credentials = "password",
                credentialType = CredentialsType.password
            };
            var serializer = new DotNetXmlSerializer();
            var serializedCredentials = serializer.Serialize(credentials);
            var restClient = MockRepository.GenerateMock<IRestClient>();
            restClient.Expect(rc =>
                rc.Execute<AuthenticationResult>(Arg<RestRequest>.Matches(r =>
                    r.Resource == "authenticate" &&
                    r.Method == Method.POST &&
                    r.HasBody(serializedCredentials))))
                .Return(null);
            var extranet = new TestExtranetUserAuthentication(restClient);

            try
            {
                extranet.Authenticate(credentials);
            }
            catch { }

            restClient.VerifyAllExpectations();
        }

        [Test]
        public void AuthenticateReturnsResultFromRestResponse()
        {
            var result = new AuthenticationResult();
            var response = new RestResponse<AuthenticationResult>() { Data = result, StatusCode = HttpStatusCode.OK };
            var restClient = MockRepository.GenerateStub<IRestClient>();
            restClient.Stub(rc => rc.Execute<AuthenticationResult>(Arg<IRestRequest>.Is.Anything)).Return(response);
            var extranet = new TestExtranetUserAuthentication(restClient);

            var actual = extranet.Authenticate("username", "password");

            Assert.That(actual, Is.EqualTo(result));
        }

        [TestCase(HttpStatusCode.BadGateway)]
        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.Forbidden)]
        [TestCase(HttpStatusCode.InternalServerError)]
        [TestCase(HttpStatusCode.NoContent)]
        [TestCase(HttpStatusCode.NotFound)]
        [TestCase(HttpStatusCode.NotImplemented)]
        [TestCase(HttpStatusCode.ServiceUnavailable)]
        public void AuthenticateThrowsRestExceptionWhenResponseStatusIsNotOk(HttpStatusCode statusCode)
        {
            var response = new RestResponse<AuthenticationResult> { StatusCode = statusCode };
            var restClient = MockRepository.GenerateStub<IRestClient>();
            restClient.Stub(rc => rc.Execute<AuthenticationResult>(Arg<IRestRequest>.Is.Anything)).Return(response);
            var extranet = new TestExtranetUserAuthentication(restClient);

            Assert.Throws<RestException>(() => extranet.Authenticate("username", "password"));
        }

        private class TestExtranetUserAuthentication : ExtranetUserAuthentication
        {
            private readonly IRestClient _restClient;
            private readonly Func<AuthenticationCredentials, AuthenticationResult> _authenticateCore;

            public IExtranetUserAuthenticationConfiguration ConfigurationForTesting
            {
                get { return base.Configuration; }
            }

            public TestExtranetUserAuthentication(IExtranetUserAuthenticationConfiguration configuration)
                : base(configuration)
            {
            }

            public TestExtranetUserAuthentication(IRestClient restClient)
            {
                _restClient = restClient;
            }

            public TestExtranetUserAuthentication(Func<AuthenticationCredentials, AuthenticationResult> authenticate)
            {
                if (authenticate != null)
                {
                    _authenticateCore = authenticate;
                }
            }

            public TestExtranetUserAuthentication()
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

            protected override AuthenticationResult AuthenticateCore(AuthenticationCredentials credentials)
            {
                if (_authenticateCore != null)
                {
                    return _authenticateCore(credentials);
                }
                return base.AuthenticateCore(credentials);
            }
        }
    }
}