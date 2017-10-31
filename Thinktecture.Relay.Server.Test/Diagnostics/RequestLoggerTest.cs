using System;
using System.Net;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
using Thinktecture.Relay.Server.Dto;
using Thinktecture.Relay.Server.Helper;
using Thinktecture.Relay.Server.OnPremise;
using Thinktecture.Relay.Server.Repository;

namespace Thinktecture.Relay.Server.Diagnostics
{
	// ReSharper disable JoinDeclarationAndInitializer
	// ReSharper disable UnusedAutoPropertyAccessor.Local
	[TestClass]
	public class RequestLoggerTest
	{
		[TestMethod]
		public void LogRequest_writes_correctly_composed_log_entry_to_the_RelayRepository_when_OnPremiseTargetResult_is_null()
		{
			var logRepositoryMock = new Mock<ILogRepository>();
			var pathSplitterMock = new Mock<IPathSplitter>();
			IRequestLogger sut = new RequestLogger(logRepositoryMock.Object, pathSplitterMock.Object);
			var clientRequest = new OnPremiseConnectorRequest
			{
				Body = new byte[] { 0, 0, 0 },
				RequestStarted = new DateTime(2014, 1, 1),
				RequestFinished = new DateTime(2014, 1, 2)
			};
			RequestLogEntry result = null;

			logRepositoryMock.Setup(r => r.LogRequest(It.IsAny<RequestLogEntry>())).Callback<RequestLogEntry>(r => result = r);
			pathSplitterMock.Setup(p => p.Split(It.IsAny<string>())).Returns(new PathInformation { OnPremiseTargetKey = "that", LocalUrl = "/file.html" });

			sut.LogRequest(clientRequest, null, Guid.Parse("4bb4ff98-ba03-49ee-bd83-5a229f63fade"), new Guid("35eff886-2d7c-4265-a6a4-f3f471ab93e8"), "gimme/that/file.html", HttpStatusCode.OK);

			logRepositoryMock.Verify(r => r.LogRequest(It.IsAny<RequestLogEntry>()));
			result.HttpStatusCode.Should().Be(HttpStatusCode.PaymentRequired);
			result.OriginId.Should().Be(Guid.Parse("35eff886-2d7c-4265-a6a4-f3f471ab93e8"));
			result.LocalUrl.Should().Be("/file.html");
			result.OnPremiseTargetKey.Should().Be("that");
			result.OnPremiseConnectorInDate.Should().Be(new DateTime(2014, 1, 1));
			result.OnPremiseConnectorOutDate.Should().Be(new DateTime(2014, 1, 2));
			result.OnPremiseTargetInDate.Should().Be(null);
			result.OnPremiseTargetOutDate.Should().Be(null);
			result.ContentBytesIn.Should().Be(3L);
			result.ContentBytesOut.Should().Be(0L);
			result.LinkId.Should().Be(Guid.Parse("4bb4ff98-ba03-49ee-bd83-5a229f63fade"));
		}

		[TestMethod]
		public void LogRequest_writes_correctly_composed_log_entry_to_the_RelayRepository_with_OnPremiseTargetResult_set()
		{
			var logRepositoryMock = new Mock<ILogRepository>();
			var pathSplitterMock = new Mock<IPathSplitter>();
			IRequestLogger sut = new RequestLogger(logRepositoryMock.Object, pathSplitterMock.Object);
			var clientRequest = new OnPremiseConnectorRequest
			{
				Body = new byte[] { 0, 0, 0 },
				RequestStarted = new DateTime(2014, 1, 1),
				RequestFinished = new DateTime(2014, 1, 2)
			};
			var onPremiseConnectorResponse = new OnPremiseConnectorResponse
			{
				RequestStarted = new DateTime(2014, 1, 3),
				RequestFinished = new DateTime(2014, 1, 4),
			};
			RequestLogEntry result = null;

			logRepositoryMock.Setup(r => r.LogRequest(It.IsAny<RequestLogEntry>())).Callback<RequestLogEntry>(r => result = r);
			pathSplitterMock.Setup(p => p.Split(It.IsAny<string>())).Returns(new PathInformation { OnPremiseTargetKey = "that", LocalUrl = "/file.html" });

			sut.LogRequest(clientRequest, onPremiseConnectorResponse, Guid.Parse("4bb4ff98-ba03-49ee-bd83-5a229f63fade"), new Guid("35eff886-2d7c-4265-a6a4-f3f471ab93e8"), "gimme/that/file.html", HttpStatusCode.OK);

			logRepositoryMock.Verify(r => r.LogRequest(It.IsAny<RequestLogEntry>()));
			result.HttpStatusCode.Should().Be(HttpStatusCode.PaymentRequired);
			result.OriginId.Should().Be(Guid.Parse("35eff886-2d7c-4265-a6a4-f3f471ab93e8"));
			result.LocalUrl.Should().Be("/file.html");
			result.OnPremiseTargetKey.Should().Be("that");
			result.OnPremiseConnectorInDate.Should().Be(new DateTime(2014, 1, 1));
			result.OnPremiseConnectorOutDate.Should().Be(new DateTime(2014, 1, 2));
			result.OnPremiseTargetInDate.Should().Be(new DateTime(2014, 1, 3));
			result.OnPremiseTargetOutDate.Should().Be(new DateTime(2014, 1, 4));
			result.ContentBytesIn.Should().Be(3L);
			result.ContentBytesOut.Should().Be(2L);
			result.LinkId.Should().Be(Guid.Parse("4bb4ff98-ba03-49ee-bd83-5a229f63fade"));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void LogRequest_throws_an_exception_if_clientRequest_is_null()
		{
			IRequestLogger sut = new RequestLogger(null, null);

			sut.LogRequest(null, null, Guid.Parse("4bb4ff98-ba03-49ee-bd83-5a229f63fade"), new Guid("35eff886-2d7c-4265-a6a4-f3f471ab93e8"), "gimme/that/file.html", HttpStatusCode.OK);
		}
	}
}
