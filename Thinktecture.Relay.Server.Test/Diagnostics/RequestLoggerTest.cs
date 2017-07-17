using System;
using System.Collections.Generic;
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
		public void GetContentByteCount_returns_0_when_content_byte_array_is_null()
		{
			var sut = new RequestLogger(null, null);
			long result;

			result = sut.GetContentByteCount(null);

			result.Should().Be(0L);
		}

		[TestMethod]
		public void GetContentByteCount_returns_the_length_of_a_given_byte_array()
		{
			var sut = new RequestLogger(null, null);
			long result;

			result = sut.GetContentByteCount(new byte[] { 0, 0, 0, 0 });

			result.Should().Be(4L);
		}

		[TestMethod]
		public void GetOnPremiseTargetInformation_returns_default_values_if_OnPremiseTargetResult_is_null()
		{
			var sut = new RequestLogger(null, null);
			RequestLogger.OnPremiseTargetInformation result;

			result = sut.GetOnPremiseTargetInformation(null);

			result.ContentBytesOut.Should().Be(0L);
			result.OnPremiseTargetInDate.Should().Be(null);
			result.OnPremiseTargetOutDate.Should().Be(null);
		}

		[TestMethod]
		public void GetOnPremiseTargetInformation_returns_date_values_from_OnPremiseTargetResult_when_content_is_null()
		{
			var onPremiseTargetResponse = new OnPremiseTargetResponse
			{
				RequestStarted = new DateTime(2014, 1, 1),
				RequestFinished = new DateTime(2014, 1, 2)
			};
			var sut = new RequestLogger(null, null);
			RequestLogger.OnPremiseTargetInformation result;

			result = sut.GetOnPremiseTargetInformation(onPremiseTargetResponse);

			result.ContentBytesOut.Should().Be(0L);
			result.OnPremiseTargetInDate.Should().Be(new DateTime(2014, 1, 1));
			result.OnPremiseTargetOutDate.Should().Be(new DateTime(2014, 1, 2));
		}

		[TestMethod]
		public void GetOnPremiseTargetInformation_returns_values_from_OnPremiseTargetResult()
		{
			var onPremiseTargetResponse = new OnPremiseTargetResponse
			{
				RequestStarted = new DateTime(2014, 1, 1),
				RequestFinished = new DateTime(2014, 1, 2),
				Body = new byte[] { 0, 0, 0, 0 }
			};
			var sut = new RequestLogger(null, null);
			RequestLogger.OnPremiseTargetInformation result;

			result = sut.GetOnPremiseTargetInformation(onPremiseTargetResponse);

			result.ContentBytesOut.Should().Be(4L);
			result.OnPremiseTargetInDate.Should().Be(new DateTime(2014, 1, 1));
			result.OnPremiseTargetOutDate.Should().Be(new DateTime(2014, 1, 2));
		}

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

			sut.LogRequest(clientRequest, null, HttpStatusCode.PaymentRequired, Guid.Parse("4bb4ff98-ba03-49ee-bd83-5a229f63fade"), "35eff886-2d7c-4265-a6a4-f3f471ab93e8", "gimme/that/file.html");

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
			var onPremiseTargetResponse = new OnPremiseTargetResponse
			{
				RequestStarted = new DateTime(2014,1,3),
				RequestFinished = new DateTime(2014,1,4),
				Body = new byte[] { 0, 0 }
			};
			RequestLogEntry result = null;

			logRepositoryMock.Setup(r => r.LogRequest(It.IsAny<RequestLogEntry>())).Callback<RequestLogEntry>(r => result = r);
			pathSplitterMock.Setup(p => p.Split(It.IsAny<string>())).Returns(new PathInformation { OnPremiseTargetKey = "that", LocalUrl = "/file.html" });

			sut.LogRequest(clientRequest, onPremiseTargetResponse, HttpStatusCode.PaymentRequired, Guid.Parse("4bb4ff98-ba03-49ee-bd83-5a229f63fade"), "35eff886-2d7c-4265-a6a4-f3f471ab93e8", "gimme/that/file.html");

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

			sut.LogRequest(null, null, HttpStatusCode.PaymentRequired, Guid.Parse("4bb4ff98-ba03-49ee-bd83-5a229f63fade"), "35eff886-2d7c-4265-a6a4-f3f471ab93e8", "gimme/that/file.html");
		}
	}
}
