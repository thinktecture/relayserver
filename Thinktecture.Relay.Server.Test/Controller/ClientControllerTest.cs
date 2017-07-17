using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
using Thinktecture.Relay.Server.Communication;
using Thinktecture.Relay.Server.Diagnostics;
using Thinktecture.Relay.Server.Dto;
using Thinktecture.Relay.Server.Helper;
using Thinktecture.Relay.Server.Http;
using Thinktecture.Relay.Server.OnPremise;
using Thinktecture.Relay.Server.Repository;

namespace Thinktecture.Relay.Server.Controller
{
	// ReSharper disable JoinDeclarationAndInitializer
	// ReSharper disable UnusedAutoPropertyAccessor.Local
	[TestClass]
	public class ClientControllerTest
	{
		private class OnPremiseTargetResponse : IOnPremiseTargetResponse
		{
			public string RequestId { get; set; }
			public string OriginId { get; set; }
			public IDictionary<string, string> HttpHeaders { get; set; }
			public HttpStatusCode StatusCode { get; set; }
			public byte[] Body { get; set; }
			public DateTime RequestStarted { get; set; }
			public DateTime RequestFinished { get; set; }
		}

		[TestMethod]
		public async Task Relay_delegates_information_and_returns_valid_httpResponseMessage_when_entered_a_valid_path()
		{
			var startTime = DateTime.UtcNow;
			var loggerMock = new Mock<ILogger>();
			var backendCommunicationMock = new Mock<IBackendCommunication>();
			var relayRepositoryMock = new Mock<ILinkRepository>();
			var pathSplitterMock = new Mock<IPathSplitter>();
			var clientRequestBuilderMock = new Mock<IOnPremiseRequestBuilder>();
			var httpResponseMessageBuilderMock = new Mock<IHttpResponseMessageBuilder>();
			var requestLoggerMock = new Mock<IRequestLogger>();
			var traceManagerMock = new Mock<ITraceManager>();
			var sut = new ClientController(backendCommunicationMock.Object, loggerMock.Object, relayRepositoryMock.Object, requestLoggerMock.Object, httpResponseMessageBuilderMock.Object, clientRequestBuilderMock.Object, pathSplitterMock.Object, traceManagerMock.Object)
			{
				ControllerContext = new HttpControllerContext { Request = new HttpRequestMessage { Method = HttpMethod.Post } },
				Request = new HttpRequestMessage()
			};
			HttpResponseMessage result;

			var linkFake = new Link { Id = Guid.Parse("fb35e2fb-5fb6-4475-baa0-e0b06f5fdeda") };
			var clientRequestFake = new OnPremiseConnectorRequest { RequestId = "239b6e03-9795-450d-bdd1-ab72900f1a98" };
			var onPremiseTargetReponseFake = new OnPremiseTargetResponse();
			var httpResponseMessageFake = new HttpResponseMessage { StatusCode = HttpStatusCode.Found };
			var localConfigurationGuid = Guid.NewGuid();

			loggerMock.Setup(l => l.Trace(It.IsAny<string>));
			backendCommunicationMock.SetupGet(b => b.OriginId).Returns("c9208bdb-c195-460d-b84e-6c146bb252e5");
			relayRepositoryMock.Setup(l => l.GetLink(It.IsAny<string>())).Returns(linkFake);
			pathSplitterMock.Setup(p => p.Split(It.IsAny<string>())).Returns(new PathInformation { PathWithoutUserName = "Bar/Baz" });
			clientRequestBuilderMock.Setup(c => c.BuildFrom(sut.Request, "c9208bdb-c195-460d-b84e-6c146bb252e5", "Bar/Baz")).ReturnsAsync(clientRequestFake);
			backendCommunicationMock.Setup(b => b.SendOnPremiseConnectorRequest("fb35e2fb-5fb6-4475-baa0-e0b06f5fdeda", clientRequestFake)).Returns(Task.FromResult(0));
			backendCommunicationMock.Setup(b => b.GetResponseAsync("239b6e03-9795-450d-bdd1-ab72900f1a98")).ReturnsAsync(onPremiseTargetReponseFake);
			httpResponseMessageBuilderMock.Setup(h => h.BuildFrom(onPremiseTargetReponseFake, linkFake)).Returns(httpResponseMessageFake);
			traceManagerMock.Setup(t => t.GetCurrentTraceConfigurationId(linkFake.Id)).Returns(localConfigurationGuid);
			traceManagerMock.Setup(t => t.Trace(clientRequestFake, onPremiseTargetReponseFake, localConfigurationGuid));
			requestLoggerMock.Setup(r => r.LogRequest(clientRequestFake, onPremiseTargetReponseFake, HttpStatusCode.Found, Guid.Parse("fb35e2fb-5fb6-4475-baa0-e0b06f5fdeda"), "c9208bdb-c195-460d-b84e-6c146bb252e5", "Foo/Bar/Baz"));

			result = await sut.Relay("Foo/Bar/Baz");

			relayRepositoryMock.VerifyAll();
			pathSplitterMock.VerifyAll();
			backendCommunicationMock.VerifyAll();
			clientRequestBuilderMock.VerifyAll();
			httpResponseMessageBuilderMock.VerifyAll();
			requestLoggerMock.VerifyAll();
			traceManagerMock.VerifyAll();
			clientRequestFake.RequestFinished.Should().BeAfter(startTime).And.BeOnOrBefore(DateTime.UtcNow);
			result.Should().BeSameAs(httpResponseMessageFake);
		}

		[TestMethod]
		public async Task Relay_returns_NotFound_status_result_when_path_is_null()
		{
			var loggerMock = new Mock<ILogger>();
			var sut = new ClientController(null, loggerMock.Object, null, null, null, null, null, null)
			{
				ControllerContext = new HttpControllerContext { Request = new HttpRequestMessage { Method = HttpMethod.Post } }
			};
			HttpResponseMessage result;

			loggerMock.Setup(l => l.Trace(It.IsAny<string>));

			result = await sut.Relay(null);

			result.StatusCode.Should().Be(HttpStatusCode.NotFound);
		}

		[TestMethod]
		public async Task Relay_returns_NotFound_when_link_cannot_be_resolved()
		{
			var loggerMock = new Mock<ILogger>();
			var relayRepositoryMock = new Mock<ILinkRepository>();
			var pathSplitterMock = new Mock<IPathSplitter>();
			var sut = new ClientController(null, loggerMock.Object, relayRepositoryMock.Object, null, null, null, pathSplitterMock.Object, null)
			{
				ControllerContext = new HttpControllerContext { Request = new HttpRequestMessage { Method = HttpMethod.Post } }
			};
			HttpResponseMessage result;

			loggerMock.Setup(l => l.Trace(It.IsAny<string>));
			relayRepositoryMock.Setup(l => l.GetLink(It.IsAny<string>())).Returns(() => null);
			pathSplitterMock.Setup(p => p.Split(It.IsAny<string>())).Returns(new PathInformation());

			result = await sut.Relay("invalid/targetKey/foo.html");

			relayRepositoryMock.VerifyAll();
			pathSplitterMock.VerifyAll();
			result.StatusCode.Should().Be(HttpStatusCode.NotFound);
		}

		[TestMethod]
		public async Task Relay_returns_NotFound_when_link_is_disabled()
		{
			var loggerMock = new Mock<ILogger>();
			var relayRepositoryMock = new Mock<ILinkRepository>();
			var pathSplitterMock = new Mock<IPathSplitter>();
			var sut = new ClientController(null, loggerMock.Object, relayRepositoryMock.Object, null, null, null, pathSplitterMock.Object, null)
			{
				ControllerContext = new HttpControllerContext { Request = new HttpRequestMessage { Method = HttpMethod.Post } }
			};
			HttpResponseMessage result;

			loggerMock.Setup(l => l.Trace(It.IsAny<string>));
			relayRepositoryMock.Setup(l => l.GetLink(It.IsAny<string>())).Returns(new Link { IsDisabled = true });
			pathSplitterMock.Setup(p => p.Split(It.IsAny<string>())).Returns(new PathInformation());

			result = await sut.Relay("invalid/targetKey/foo.html");

			relayRepositoryMock.VerifyAll();
			pathSplitterMock.VerifyAll();
			result.StatusCode.Should().Be(HttpStatusCode.NotFound);
		}

		[TestMethod]
		public async Task Relay_returns_NotFound_when_path_without_username_is_null()
		{
			var loggerMock = new Mock<ILogger>();
			var relayRepositoryMock = new Mock<ILinkRepository>();
			var pathSplitterMock = new Mock<IPathSplitter>();
			var sut = new ClientController(null, loggerMock.Object, relayRepositoryMock.Object, null, null, null, pathSplitterMock.Object, null)
			{
				ControllerContext = new HttpControllerContext { Request = new HttpRequestMessage { Method = HttpMethod.Post } }
			};
			HttpResponseMessage result;

			loggerMock.Setup(l => l.Trace(It.IsAny<string>));
			relayRepositoryMock.Setup(l => l.GetLink(It.IsAny<string>())).Returns(new Link());
			pathSplitterMock.Setup(p => p.Split(It.IsAny<string>())).Returns(new PathInformation());

			result = await sut.Relay("invalid");

			relayRepositoryMock.VerifyAll();
			pathSplitterMock.VerifyAll();
			result.StatusCode.Should().Be(HttpStatusCode.NotFound);
		}

		[TestMethod]
		public async Task Relay_returns_NotFound_when_path_without_username_is_whitespace()
		{
			var loggerMock = new Mock<ILogger>();
			var relayRepositoryMock = new Mock<ILinkRepository>();
			var pathSplitterMock = new Mock<IPathSplitter>();
			var sut = new ClientController(null, loggerMock.Object, relayRepositoryMock.Object, null, null, null, pathSplitterMock.Object, null)
			{
				ControllerContext = new HttpControllerContext { Request = new HttpRequestMessage { Method = HttpMethod.Post } }
			};
			HttpResponseMessage result;

			loggerMock.Setup(l => l.Trace(It.IsAny<string>));
			relayRepositoryMock.Setup(l => l.GetLink(It.IsAny<string>())).Returns(new Link());
			pathSplitterMock.Setup(p => p.Split(It.IsAny<string>())).Returns(new PathInformation { PathWithoutUserName = "    " });

			result = await sut.Relay("invalid/      ");

			relayRepositoryMock.VerifyAll();
			pathSplitterMock.VerifyAll();
			result.StatusCode.Should().Be(HttpStatusCode.NotFound);
		}

		[TestMethod]
		public async Task Relay_returns_NotFound_when_path_when_client_request_is_not_local_and_external_requests_are_disallowed_by_link()
		{
			var loggerMock = new Mock<ILogger>();
			var relayRepositoryMock = new Mock<ILinkRepository>();
			var pathSplitterMock = new Mock<IPathSplitter>();
			var sut = new ClientController(null, loggerMock.Object, relayRepositoryMock.Object, null, null, null, pathSplitterMock.Object, null)
			{
				ControllerContext = new HttpControllerContext { Request = new HttpRequestMessage { Method = HttpMethod.Post } },
				Request = new HttpRequestMessage()
			};
			HttpResponseMessage result;

			loggerMock.Setup(l => l.Trace(It.IsAny<string>));
			relayRepositoryMock.Setup(l => l.GetLink(It.IsAny<string>())).Returns(new Link { AllowLocalClientRequestsOnly = true });
			pathSplitterMock.Setup(p => p.Split(It.IsAny<string>())).Returns(new PathInformation { PathWithoutUserName = "Bar/Baz" });

			result = await sut.Relay("Foo/Bar/Baz");

			relayRepositoryMock.VerifyAll();
			pathSplitterMock.VerifyAll();
			result.StatusCode.Should().Be(HttpStatusCode.NotFound);
		}

		[TestMethod]
		public async Task Relay_does_not_trace_if_no_tracing_is_configured_for_link()
		{
			var startTime = DateTime.UtcNow;
			var loggerMock = new Mock<ILogger>();
			var backendCommunicationMock = new Mock<IBackendCommunication>();
			var relayRepositoryMock = new Mock<ILinkRepository>();
			var pathSplitterMock = new Mock<IPathSplitter>();
			var clientRequestBuilderMock = new Mock<IOnPremiseRequestBuilder>();
			var httpResponseMessageBuilderMock = new Mock<IHttpResponseMessageBuilder>();
			var requestLoggerMock = new Mock<IRequestLogger>();
			var traceManagerMock = new Mock<ITraceManager>();
			var sut = new ClientController(backendCommunicationMock.Object, loggerMock.Object, relayRepositoryMock.Object, requestLoggerMock.Object, httpResponseMessageBuilderMock.Object, clientRequestBuilderMock.Object, pathSplitterMock.Object, traceManagerMock.Object)
			{
				ControllerContext = new HttpControllerContext { Request = new HttpRequestMessage { Method = HttpMethod.Post } },
				Request = new HttpRequestMessage()
			};
			HttpResponseMessage result;

			var linkFake = new Link { Id = Guid.Parse("fb35e2fb-5fb6-4475-baa0-e0b06f5fdeda") };
			var clientRequestFake = new OnPremiseConnectorRequest { RequestId = "239b6e03-9795-450d-bdd1-ab72900f1a98" };
			var onPremiseTargetReponseFake = new OnPremiseTargetResponse();
			var httpResponseMessageFake = new HttpResponseMessage { StatusCode = HttpStatusCode.Found };

			loggerMock.Setup(l => l.Trace(It.IsAny<string>));
			backendCommunicationMock.SetupGet(b => b.OriginId).Returns("c9208bdb-c195-460d-b84e-6c146bb252e5");
			relayRepositoryMock.Setup(l => l.GetLink(It.IsAny<string>())).Returns(linkFake);
			pathSplitterMock.Setup(p => p.Split(It.IsAny<string>())).Returns(new PathInformation { PathWithoutUserName = "Bar/Baz" });
			clientRequestBuilderMock.Setup(c => c.BuildFrom(sut.Request, "c9208bdb-c195-460d-b84e-6c146bb252e5", "Bar/Baz")).ReturnsAsync(clientRequestFake);
			backendCommunicationMock.Setup(b => b.SendOnPremiseConnectorRequest("fb35e2fb-5fb6-4475-baa0-e0b06f5fdeda", clientRequestFake)).Returns(Task.FromResult(0));
			backendCommunicationMock.Setup(b => b.GetResponseAsync("239b6e03-9795-450d-bdd1-ab72900f1a98")).ReturnsAsync(onPremiseTargetReponseFake);
			httpResponseMessageBuilderMock.Setup(h => h.BuildFrom(onPremiseTargetReponseFake, linkFake)).Returns(httpResponseMessageFake);
			traceManagerMock.Setup(t => t.GetCurrentTraceConfigurationId(linkFake.Id)).Returns((Guid?) null);
			requestLoggerMock.Setup(r => r.LogRequest(clientRequestFake, onPremiseTargetReponseFake, HttpStatusCode.Found, Guid.Parse("fb35e2fb-5fb6-4475-baa0-e0b06f5fdeda"), "c9208bdb-c195-460d-b84e-6c146bb252e5", "Foo/Bar/Baz"));

			result = await sut.Relay("Foo/Bar/Baz");

			relayRepositoryMock.VerifyAll();
			pathSplitterMock.VerifyAll();
			backendCommunicationMock.VerifyAll();
			clientRequestBuilderMock.VerifyAll();
			httpResponseMessageBuilderMock.VerifyAll();
			requestLoggerMock.VerifyAll();
			traceManagerMock.VerifyAll();
			clientRequestFake.RequestFinished.Should().BeOnOrAfter(startTime).And.BeOnOrBefore(DateTime.UtcNow);
			result.Should().BeSameAs(httpResponseMessageFake);
		}
	}
}