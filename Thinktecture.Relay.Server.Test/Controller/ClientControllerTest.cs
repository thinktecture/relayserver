using System;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Serilog;
using Thinktecture.Relay.Server.Communication;
using Thinktecture.Relay.Server.Diagnostics;
using Thinktecture.Relay.Server.Dto;
using Thinktecture.Relay.Server.Helper;
using Thinktecture.Relay.Server.Http;
using Thinktecture.Relay.Server.Interceptor;
using Thinktecture.Relay.Server.OnPremise;
using Thinktecture.Relay.Server.Repository;

namespace Thinktecture.Relay.Server.Controller
{
	// ReSharper disable JoinDeclarationAndInitializer
	// ReSharper disable UnusedAutoPropertyAccessor.Local
	[TestClass]
	public class ClientControllerTest
	{
		private readonly Mock<ILogger> _loggerMock;
		private readonly Mock<IBackendCommunication> _backendCommunicationMock;
		private readonly Mock<ILinkRepository> _relayRepositoryMock;
		private readonly Mock<IPathSplitter> _pathSplitterMock;
		private readonly Mock<IOnPremiseRequestBuilder> _clientRequestBuilderMock;
		private readonly Mock<IHttpResponseMessageBuilder> _httpResponseMessageBuilderMock;
		private readonly Mock<IRequestLogger> _requestLoggerMock;
		private readonly Mock<ITraceManager> _traceManagerMock;
		private readonly Mock<IInterceptorManager> _interceptorManagerMock;
		private readonly Mock<IPostDataTemporaryStore> _postDataTemporaryStoreMock;

		private class FakePrincipal : IPrincipal
		{
			public FakePrincipal()
			{
				Identity = new FakeIdentity();
			}

			public bool IsInRole(string role)
			{
				return false;
			}

			public IIdentity Identity { get; }
		}

		private class FakeIdentity : IIdentity
		{
			public string Name { get; }
			public string AuthenticationType { get; }
			public bool IsAuthenticated { get; }
		}

		public ClientControllerTest()
		{
			_loggerMock = new Mock<ILogger>();
			_backendCommunicationMock = new Mock<IBackendCommunication>();
			_relayRepositoryMock = new Mock<ILinkRepository>();
			_pathSplitterMock = new Mock<IPathSplitter>();
			_clientRequestBuilderMock = new Mock<IOnPremiseRequestBuilder>();
			_httpResponseMessageBuilderMock = new Mock<IHttpResponseMessageBuilder>();
			_requestLoggerMock = new Mock<IRequestLogger>();
			_traceManagerMock = new Mock<ITraceManager>();
			_interceptorManagerMock = new Mock<IInterceptorManager>();
			_postDataTemporaryStoreMock = new Mock<IPostDataTemporaryStore>();
		}

		private ClientController CreateClientController()
		{
			return new ClientController(_backendCommunicationMock.Object, _loggerMock.Object, _relayRepositoryMock.Object, _requestLoggerMock.Object,
				_httpResponseMessageBuilderMock.Object, _clientRequestBuilderMock.Object, _pathSplitterMock.Object, _traceManagerMock.Object, _interceptorManagerMock.Object,
				_postDataTemporaryStoreMock.Object);
		}

		[TestMethod]
		public async Task Relay_delegates_information_and_returns_valid_httpResponseMessage_when_entered_a_valid_path()
		{
			var startTime = DateTime.UtcNow;
			var sut = CreateClientController();
			sut.ControllerContext = new HttpControllerContext { Request = new HttpRequestMessage { Method = HttpMethod.Post } };
			sut.Request = new HttpRequestMessage();
			sut.User = new FakePrincipal();

			HttpResponseMessage result;

			var requestId = "239b6e03-9795-450d-bdd1-ab72900f1a98";
			var linkId = new Guid("fb35e2fb-5fb6-4475-baa0-e0b06f5fdeda");
			var linkFake = new Link { Id = linkId };
			var clientRequestFake = new OnPremiseConnectorRequest { RequestId = requestId };
			var onPremiseTargetReponseFake = new OnPremiseConnectorResponse();
			var httpResponseMessageFake = new HttpResponseMessage { StatusCode = HttpStatusCode.Found };
			var localConfigurationGuid = Guid.NewGuid();
			var originId = new Guid("c9208bdb-c195-460d-b84e-6c146bb252e5");

			_loggerMock.Setup(l => l.Verbose(It.IsAny<string>()));
			_backendCommunicationMock.SetupGet(b => b.OriginId).Returns(originId);
			_relayRepositoryMock.Setup(l => l.GetLink(It.IsAny<string>())).Returns(linkFake);
			_pathSplitterMock.Setup(p => p.Split(It.IsAny<string>())).Returns(new PathInformation { PathWithoutUserName = "Bar/Baz" });
			_clientRequestBuilderMock.Setup(c => c.BuildFromHttpRequest(sut.Request, originId, "Bar/Baz")).ReturnsAsync(clientRequestFake);
			_backendCommunicationMock.Setup(b => b.SendOnPremiseConnectorRequest(linkId, clientRequestFake));
			_backendCommunicationMock.Setup(b => b.GetResponseAsync(requestId, null)).ReturnsAsync(onPremiseTargetReponseFake);
			_httpResponseMessageBuilderMock.Setup(h => h.BuildFromConnectorResponse(onPremiseTargetReponseFake, linkFake, requestId)).Returns(httpResponseMessageFake);
			_traceManagerMock.Setup(t => t.GetCurrentTraceConfigurationId(linkFake.Id)).Returns(localConfigurationGuid);
			_traceManagerMock.Setup(t => t.Trace(clientRequestFake, onPremiseTargetReponseFake, localConfigurationGuid));
			_requestLoggerMock.Setup(r => r.LogRequest(clientRequestFake, onPremiseTargetReponseFake, linkId, originId, "Foo/Bar/Baz", 0));
			HttpResponseMessage messageDummy;
			_interceptorManagerMock.Setup(i => i.HandleRequest(clientRequestFake, sut.Request, sut.User, out messageDummy)).Returns(clientRequestFake);
			_interceptorManagerMock.Setup(i => i.HandleResponse(clientRequestFake, sut.Request, sut.User, onPremiseTargetReponseFake)).Returns((HttpResponseMessage)null);

			result = await sut.Relay("Foo/Bar/Baz");

			_relayRepositoryMock.VerifyAll();
			_pathSplitterMock.VerifyAll();
			_backendCommunicationMock.VerifyAll();
			_clientRequestBuilderMock.VerifyAll();
			_httpResponseMessageBuilderMock.VerifyAll();
			_requestLoggerMock.VerifyAll();
			_traceManagerMock.VerifyAll();
			_interceptorManagerMock.VerifyAll();
			clientRequestFake.RequestFinished.Should().BeAfter(startTime).And.BeOnOrBefore(DateTime.UtcNow);
			result.Should().BeSameAs(httpResponseMessageFake);
		}

		[TestMethod]
		public async Task Relay_returns_NotFound_status_result_when_path_is_null()
		{
			var sut = CreateClientController();
			sut.ControllerContext = new HttpControllerContext { Request = new HttpRequestMessage { Method = HttpMethod.Post } };
			HttpResponseMessage result;

			_loggerMock.Setup(l => l.Verbose(It.IsAny<string>()));

			result = await sut.Relay(null);

			result.StatusCode.Should().Be(HttpStatusCode.NotFound);
		}

		[TestMethod]
		public async Task Relay_returns_NotFound_when_link_cannot_be_resolved()
		{
			var sut = CreateClientController();
			sut.ControllerContext = new HttpControllerContext { Request = new HttpRequestMessage { Method = HttpMethod.Post } };
			HttpResponseMessage result;

			_loggerMock.Setup(l => l.Verbose(It.IsAny<string>()));
			_relayRepositoryMock.Setup(l => l.GetLink(It.IsAny<string>())).Returns(() => null);
			_pathSplitterMock.Setup(p => p.Split(It.IsAny<string>())).Returns(new PathInformation());

			result = await sut.Relay("invalid/targetKey/foo.html");

			_relayRepositoryMock.VerifyAll();
			_pathSplitterMock.VerifyAll();
			result.StatusCode.Should().Be(HttpStatusCode.NotFound);
		}

		[TestMethod]
		public async Task Relay_returns_NotFound_when_link_is_disabled()
		{
			var sut = CreateClientController();
			sut.ControllerContext = new HttpControllerContext { Request = new HttpRequestMessage { Method = HttpMethod.Post } };
			HttpResponseMessage result;

			_loggerMock.Setup(l => l.Verbose(It.IsAny<string>()));
			_relayRepositoryMock.Setup(l => l.GetLink(It.IsAny<string>())).Returns(new Link { IsDisabled = true });
			_pathSplitterMock.Setup(p => p.Split(It.IsAny<string>())).Returns(new PathInformation());

			result = await sut.Relay("invalid/targetKey/foo.html");

			_relayRepositoryMock.VerifyAll();
			_pathSplitterMock.VerifyAll();
			result.StatusCode.Should().Be(HttpStatusCode.NotFound);
		}

		[TestMethod]
		public async Task Relay_returns_NotFound_when_path_without_username_is_null()
		{
			var sut = CreateClientController();
			sut.ControllerContext = new HttpControllerContext { Request = new HttpRequestMessage { Method = HttpMethod.Post } };
			HttpResponseMessage result;

			_loggerMock.Setup(l => l.Verbose(It.IsAny<string>()));
			_relayRepositoryMock.Setup(l => l.GetLink(It.IsAny<string>())).Returns(new Link());
			_pathSplitterMock.Setup(p => p.Split(It.IsAny<string>())).Returns(new PathInformation());

			result = await sut.Relay("invalid");

			_relayRepositoryMock.VerifyAll();
			_pathSplitterMock.VerifyAll();
			result.StatusCode.Should().Be(HttpStatusCode.NotFound);
		}

		[TestMethod]
		public async Task Relay_returns_NotFound_when_path_without_username_is_whitespace()
		{
			var sut = CreateClientController();
			sut.ControllerContext = new HttpControllerContext { Request = new HttpRequestMessage { Method = HttpMethod.Post } };
			HttpResponseMessage result;

			_loggerMock.Setup(l => l.Verbose(It.IsAny<string>()));
			_relayRepositoryMock.Setup(l => l.GetLink(It.IsAny<string>())).Returns(new Link());
			_pathSplitterMock.Setup(p => p.Split(It.IsAny<string>())).Returns(new PathInformation { PathWithoutUserName = "    " });

			result = await sut.Relay("invalid/      ");

			_relayRepositoryMock.VerifyAll();
			_pathSplitterMock.VerifyAll();
			result.StatusCode.Should().Be(HttpStatusCode.NotFound);
		}

		[TestMethod]
		public async Task Relay_returns_NotFound_when_path_when_client_request_is_not_local_and_external_requests_are_disallowed_by_link()
		{
			var sut = CreateClientController();
			sut.ControllerContext = new HttpControllerContext { Request = new HttpRequestMessage { Method = HttpMethod.Post } };
			sut.Request = new HttpRequestMessage();
			HttpResponseMessage result;

			_loggerMock.Setup(l => l.Verbose(It.IsAny<string>()));
			_relayRepositoryMock.Setup(l => l.GetLink(It.IsAny<string>())).Returns(new Link { AllowLocalClientRequestsOnly = true });
			_pathSplitterMock.Setup(p => p.Split(It.IsAny<string>())).Returns(new PathInformation { PathWithoutUserName = "Bar/Baz" });

			result = await sut.Relay("Foo/Bar/Baz");

			_relayRepositoryMock.VerifyAll();
			_pathSplitterMock.VerifyAll();
			result.StatusCode.Should().Be(HttpStatusCode.NotFound);
		}

		[TestMethod]
		public async Task Relay_does_not_trace_if_no_tracing_is_configured_for_link()
		{
			var startTime = DateTime.UtcNow;
			var sut = CreateClientController();
			sut.ControllerContext = new HttpControllerContext { Request = new HttpRequestMessage { Method = HttpMethod.Post } };
			sut.Request = new HttpRequestMessage();
			sut.User = new FakePrincipal();
			HttpResponseMessage result;

			var linkFake = new Link { Id = Guid.Parse("fb35e2fb-5fb6-4475-baa0-e0b06f5fdeda") };
			var clientRequestFake = new OnPremiseConnectorRequest { RequestId = "239b6e03-9795-450d-bdd1-ab72900f1a98" };
			var onPremiseTargetReponseFake = new OnPremiseConnectorResponse();
			var httpResponseMessageFake = new HttpResponseMessage { StatusCode = HttpStatusCode.Found };

			_loggerMock.Setup(l => l.Verbose(It.IsAny<string>()));
			_backendCommunicationMock.SetupGet(b => b.OriginId).Returns(new Guid("c9208bdb-c195-460d-b84e-6c146bb252e5"));
			_relayRepositoryMock.Setup(l => l.GetLink(It.IsAny<string>())).Returns(linkFake);
			_pathSplitterMock.Setup(p => p.Split(It.IsAny<string>())).Returns(new PathInformation { PathWithoutUserName = "Bar/Baz" });
			_clientRequestBuilderMock.Setup(c => c.BuildFromHttpRequest(sut.Request, new Guid("c9208bdb-c195-460d-b84e-6c146bb252e5"), "Bar/Baz")).ReturnsAsync(clientRequestFake);
			_backendCommunicationMock.Setup(b => b.SendOnPremiseConnectorRequest(new Guid("fb35e2fb-5fb6-4475-baa0-e0b06f5fdeda"), clientRequestFake));
			_backendCommunicationMock.Setup(b => b.GetResponseAsync("239b6e03-9795-450d-bdd1-ab72900f1a98", null)).ReturnsAsync(onPremiseTargetReponseFake);
			_httpResponseMessageBuilderMock.Setup(h => h.BuildFromConnectorResponse(onPremiseTargetReponseFake, linkFake, "239b6e03-9795-450d-bdd1-ab72900f1a98")).Returns(httpResponseMessageFake);
			_traceManagerMock.Setup(t => t.GetCurrentTraceConfigurationId(linkFake.Id)).Returns((Guid?)null);
			_requestLoggerMock.Setup(r => r.LogRequest(clientRequestFake, onPremiseTargetReponseFake, Guid.Parse("fb35e2fb-5fb6-4475-baa0-e0b06f5fdeda"), new Guid("c9208bdb-c195-460d-b84e-6c146bb252e5"), "Foo/Bar/Baz", 0));
			HttpResponseMessage messageDummy;
			_interceptorManagerMock.Setup(i => i.HandleRequest(clientRequestFake, sut.Request, sut.User, out messageDummy)).Returns(clientRequestFake);
			_interceptorManagerMock.Setup(i => i.HandleResponse(clientRequestFake, sut.Request, sut.User, onPremiseTargetReponseFake)).Returns((HttpResponseMessage)null);

			result = await sut.Relay("Foo/Bar/Baz");

			_relayRepositoryMock.VerifyAll();
			_pathSplitterMock.VerifyAll();
			_backendCommunicationMock.VerifyAll();
			_clientRequestBuilderMock.VerifyAll();
			_httpResponseMessageBuilderMock.VerifyAll();
			_requestLoggerMock.VerifyAll();
			_traceManagerMock.VerifyAll();
			clientRequestFake.RequestFinished.Should().BeOnOrAfter(startTime).And.BeOnOrBefore(DateTime.UtcNow);
			result.Should().BeSameAs(httpResponseMessageFake);
		}
	}
}
