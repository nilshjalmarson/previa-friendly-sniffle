using MvcContrib.TestHelper;
using NUnit.Framework;
using Previa.ExtranetUserAuthentication.Web.Controllers;
using Previa.ExtranetUserAuthentication.Web.Models;
using Rhino.Mocks;
using System;
using Previa.AuthenticationAuthorization.Models;

namespace Previa.ExtranetUserAuthentication.Web.Tests.Controllers
{
    [TestFixture]
    public class QlikViewControllerTests
    {
        private IExtranetUserAuthentication _authenticator;
        private IQlikViewTicket _qlikViewTicket;
        private IExtranetIdentityMappingProvider _identityMapper;
        private QlikViewController _controller;
        private TestControllerBuilder _builder;

        [SetUp]
        public void BeforeEachTest()
        {
            _authenticator = MockRepository.GenerateStub<IExtranetUserAuthentication>();
            _qlikViewTicket = MockRepository.GenerateStub<IQlikViewTicket>();
            _identityMapper = MockRepository.GenerateStub<IExtranetIdentityMappingProvider>();
            _controller = new QlikViewController(_authenticator, _qlikViewTicket, _identityMapper);
            _builder = new TestControllerBuilder();
            _builder.InitializeController(_controller);
        }

        [Test]
        public void SignInReturnsView()
        {
            _controller.SignIn((string)null).AssertViewRendered();
        }

        [Test]
        public void SignInReturnsViewOnModelStateError()
        {
            _controller.ModelState.AddModelError("key", "errorMessage");
            var viewModel = new CredentialsViewModel();

            string returnUrl = "www.dn.se";

            _controller.SignIn(viewModel)
                .AssertViewRendered()
                .WithViewData<CredentialsViewModel>()
                .ShouldBe(viewModel);

            _authenticator.AssertWasNotCalled(auth => auth.Authenticate(Arg<string>.Is.Anything, Arg<string>.Is.Anything));
            _authenticator.AssertWasNotCalled(auth => auth.Authenticate(Arg<AuthenticationCredentials>.Is.Anything));
        }

        [Test]
        public void SignInThrowsOnNullCredentials()
        {
            Assert.Throws<ArgumentNullException>(() => _controller.SignIn((CredentialsViewModel)null));
        }

        [Test]
        public void SignInCallsAuthenticator()
        {
            var viewModel = new CredentialsViewModel { Username = "username", Password = "password" };

            Do.Suppress(() => _controller.SignIn(viewModel));

            _authenticator.AssertWasCalled(auth => auth.Authenticate(viewModel.Username, viewModel.Password));
        }

        [Test]
        public void SignInReturnsViewWhenAuthenticationFailedDueToInvalidCredentials()
        {
            var viewModel = new CredentialsViewModel { Username = "username", Password = "password" };
            _authenticator.Stub(auth => auth.Authenticate(Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return(null);

            _controller.SignIn(viewModel)
                .AssertViewRendered()
                .WithInvalidModelState()
                .WithViewData<CredentialsViewModel>()
                .ShouldBe(viewModel);
        }

        [TestCase(AuthenticationStatus.accountLocked)]
        [TestCase(AuthenticationStatus.credentialsExpired)]
        public void SignInReturnsViewWhenAuthenticationFailedDueToInvalidAccountStatus(AuthenticationStatus status)
        {
            var viewModel = new CredentialsViewModel { Username = "username", Password = "password" };
            var authResult = new AuthenticationResult { status = status };
            _authenticator.Stub(auth => auth.Authenticate(Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return(authResult);

            _controller.SignIn(viewModel)
                .AssertViewRendered()
                .WithInvalidModelState()
                .WithViewData<CredentialsViewModel>()
                .ShouldBe(viewModel);
        }
    }
}