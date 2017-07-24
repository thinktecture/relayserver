using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
using Thinktecture.Relay.Server.Configuration;
using Thinktecture.Relay.Server.OnPremise;
using Thinktecture.Relay.Server.Repository;

namespace Thinktecture.Relay.Server.Diagnostics
{
	// ReSharper disable ConvertToConstant.Local
	// ReSharper disable JoinDeclarationAndInitializer
	[TestClass]
	public class TraceManagerTest
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

		private class Configuration : IConfiguration
		{
			public TimeSpan OnPremiseConnectorCallbackTimeout { get; private set; }
			public string RabbitMqConnectionString { get; private set; }
			public string TraceFileDirectory { get; private set; }
			public int LinkPasswordLength { get; private set; }
			public int DisconnectTimeout { get; private set; }
			public int ConnectionTimeout { get; private set; }
			public int KeepAliveInterval { get; private set; }
			public bool UseInsecureHttp { get; private set; }
			public bool EnableManagementWeb { get; private set; }
			public bool EnableRelaying { get; private set; }
			public bool EnableOnPremiseConnections { get; private set; }
			public string HostName { get; private set; }
			public int Port { get; private set; }
			public string ManagementWebLocation { get; private set; }
			public string TemporaryRequestStoragePath { get; }
			public int ActiveConnectionTimeoutInSeconds { get; }

			public Configuration()
			{
				TraceFileDirectory = "tracefiles";
			}
		}

		[TestMethod]
		public void IsCurrentlyTraced_correctly_reports_tracing_enabled_state_from_repository()
		{
			var traceRepositoryMock = new Mock<ITraceRepository>();
			var sut = new TraceManager(traceRepositoryMock.Object, null, null, null, null);
			var linkId = Guid.NewGuid();
			var traceConfigurationId = Guid.NewGuid();
			Guid? result;

			traceRepositoryMock.Setup(t => t.GetCurrentTraceConfigurationId(linkId)).Returns(traceConfigurationId);

			result = sut.GetCurrentTraceConfigurationId(linkId);

			result.Should().Be(traceConfigurationId);
		}

		[TestMethod]
		public void IsCurrentlyTraced_correctly_reports_tracing_disabled_state_from_repository()
		{
			var traceRepositoryMock = new Mock<ITraceRepository>();
			var sut = new TraceManager(traceRepositoryMock.Object, null, null, null, null);
			var linkId = Guid.NewGuid();
			Guid? result;

			traceRepositoryMock.Setup(t => t.GetCurrentTraceConfigurationId(linkId)).Returns((Guid?)null);

			result = sut.GetCurrentTraceConfigurationId(linkId);

			result.Should().NotHaveValue();
		}

		[TestMethod]
		public void Trace_creates_tracefiles_directory()
		{
			var traceFileWriterMock = new Mock<ITraceFileWriter>();
			var loggerMock = new Mock<ILogger>();
			var traceConfigurationId = Guid.NewGuid();
			var clientRequest = new OnPremiseConnectorRequest
			{
				HttpHeaders = new Dictionary<string, string>
				{
					{"Content-Type", "text/plain"},
					{"Content-Length", "700"}
				},
				Body = new byte[] { 65, 66, 67 }
			};
			var onPremiseTargetResponse = new OnPremiseTargetResponse()
			{
				HttpHeaders = new Dictionary<string, string>
				{
					{"Content-Type", "image/png"},
					{"Content-Length", "7500"}
				},
				Body = new byte[] { 66, 67, 68 }
			};
			ITraceManager sut = new TraceManager(null, traceFileWriterMock.Object, null, new Configuration(), loggerMock.Object);

			Directory.CreateDirectory("tracefiles");
			Directory.Delete("tracefiles");

			traceFileWriterMock.Setup(t => t.WriteContentFile(It.IsAny<string>(), clientRequest.Body));
			traceFileWriterMock.Setup(t => t.WriteContentFile(It.IsAny<string>(), onPremiseTargetResponse.Body));
			traceFileWriterMock.Setup(t => t.WriteHeaderFile(It.IsAny<string>(), clientRequest.HttpHeaders));
			traceFileWriterMock.Setup(t => t.WriteHeaderFile(It.IsAny<string>(), onPremiseTargetResponse.HttpHeaders));

			sut.Trace(clientRequest, onPremiseTargetResponse, traceConfigurationId);

			traceFileWriterMock.VerifyAll();
			Directory.Exists("tracefiles").Should().BeTrue();

			Directory.Delete("tracefiles");
		}

		[TestMethod]
		public void Trace_uses_correct_file_names_build_from_Trace_ID_and_DateTime_ticks()
		{
			var traceFileWriterMock = new Mock<ITraceFileWriter>();
			var loggerMock = new Mock<ILogger>();
			var traceConfigurationId = Guid.NewGuid();
			var clientRequest = new OnPremiseConnectorRequest
			{
				HttpHeaders = new Dictionary<string, string>
				{
					{"Content-Type", "text/plain"},
					{"Content-Length", "700"}
				},
				Body = new byte[] { 65, 66, 67 }
			};
			var onPremiseTargetResponse = new OnPremiseTargetResponse()
			{
				HttpHeaders = new Dictionary<string, string>
				{
					{"Content-Type", "image/png"},
					{"Content-Length", "7500"}
				},
				Body = new byte[] { 66, 67, 68 }
			};
			string clientRequestBodyFileName = null;
			string clientRequestHeadersFileName = null;
			string onPremiseTargetResponseBodyFileName = null;
			string onPremiseTargetResponseHeadersFileName = null;
			DateTime startTime;
			ITraceManager sut = new TraceManager(null, traceFileWriterMock.Object, null, new Configuration(), loggerMock.Object);

			startTime = DateTime.Now;

			traceFileWriterMock.Setup(t => t.WriteContentFile(It.IsAny<string>(), clientRequest.Body)).Callback((string f, byte[] c) => clientRequestBodyFileName = f);
			traceFileWriterMock.Setup(t => t.WriteContentFile(It.IsAny<string>(), onPremiseTargetResponse.Body)).Callback((string f, byte[] c) => onPremiseTargetResponseBodyFileName = f); ;
			traceFileWriterMock.Setup(t => t.WriteHeaderFile(It.IsAny<string>(), clientRequest.HttpHeaders)).Callback((string f, IDictionary<string, string> c) => clientRequestHeadersFileName = f); ;
			traceFileWriterMock.Setup(t => t.WriteHeaderFile(It.IsAny<string>(), onPremiseTargetResponse.HttpHeaders)).Callback((string f, IDictionary<string, string> c) => onPremiseTargetResponseHeadersFileName = f); ;

			sut.Trace(clientRequest, onPremiseTargetResponse, traceConfigurationId);

			var ticks = new DateTime(long.Parse(clientRequestBodyFileName.Split('-').Skip(5).First().Split('.').First()));
			var expectedFileNamePrefix = Path.Combine("tracefiles", traceConfigurationId + "-" + ticks.Ticks);

			traceFileWriterMock.VerifyAll();
			ticks.Should().BeOnOrAfter(startTime).And.BeOnOrBefore(DateTime.Now);
			clientRequestBodyFileName.Should().Be(expectedFileNamePrefix + ".ct.content");
			onPremiseTargetResponseBodyFileName.Should().Be(expectedFileNamePrefix + ".optt.content");
			clientRequestHeadersFileName.Should().Be(expectedFileNamePrefix + ".ct.headers");
			onPremiseTargetResponseHeadersFileName.Should().Be(expectedFileNamePrefix + ".optt.headers");
		}

		[TestMethod]
		public void Trace_logs_exception()
		{
			var traceFileWriterMock = new Mock<ITraceFileWriter>();
			var loggerMock = new Mock<ILogger>();
			var traceConfigurationId = Guid.NewGuid();
			var clientRequest = new OnPremiseConnectorRequest
			{
				HttpHeaders = new Dictionary<string, string>
				{
					{"Content-Type", "text/plain"},
					{"Content-Length", "700"}
				},
				Body = new byte[] { 65, 66, 67 }
			};
			var onPremiseTargetResponse = new OnPremiseTargetResponse()
			{
				HttpHeaders = new Dictionary<string, string>
				{
					{"Content-Type", "image/png"},
					{"Content-Length", "7500"}
				},
				Body = new byte[] { 66, 67, 68 }
			};
			ITraceManager sut = new TraceManager(null, traceFileWriterMock.Object, null, new Configuration(), loggerMock.Object);

			var exception = new Exception();

			traceFileWriterMock.Setup(t => t.WriteContentFile(It.IsAny<string>(), clientRequest.Body)).Throws(exception);
			loggerMock.Setup(l => l.Warn(exception, It.IsAny<string>()));

			sut.Trace(clientRequest, onPremiseTargetResponse, traceConfigurationId);

			traceFileWriterMock.VerifyAll();
		}

		[TestMethod]
		public async Task GetTraceFilesAsync_returns_file_info_objects_for_all_trace_files_of_a_given_prefix()
		{
			var traceFileReaderMock = new Mock<TraceFileReader>() { CallBase = true };
			var sut = new TraceManager(null, null, traceFileReaderMock.Object, new Configuration(), null);
			var filePrefix1 = "7975999f-54d9-4b21-a093-4502ea372723-635497418466831637";
			var filePrefix2 = "7975999f-54d9-4b21-a093-4502ea372723-635497418466831700";
			IEnumerable<Trace> result;

			var clientHeaders = new Dictionary<string, string>()
			{
				{"Content-Type", "text/plain"},
				{"Content-Length", "0"}
			};

			var onPremiseTargetHeaders = new Dictionary<string, string>()
			{
				{"Content-Type", "image/png"},
				{"Content-Length", "500"}
			};

			Directory.CreateDirectory("tracefiles");

			var traceFileWriter = new TraceFileWriter();
			await traceFileWriter.WriteHeaderFile("tracefiles/" + filePrefix1 + ".ct.headers", clientHeaders);
			await traceFileWriter.WriteHeaderFile("tracefiles/" + filePrefix1 + ".optt.headers", onPremiseTargetHeaders);
			await traceFileWriter.WriteHeaderFile("tracefiles/" + filePrefix2 + ".ct.headers", clientHeaders);
			await traceFileWriter.WriteHeaderFile("tracefiles/" + filePrefix2 + ".optt.headers", onPremiseTargetHeaders);

			result = await sut.GetTracesAsync(Guid.Parse("7975999f-54d9-4b21-a093-4502ea372723"));

			Directory.Delete("tracefiles", true);

			result.Count().Should().Be(2);
		}

		[TestMethod]
		public async Task GetTraceFilesAsync_catches_errors_when_reading_a_file_and_logs_them()
		{
			var loggerMock = new Mock<ILogger>();
			var traceFileReaderMock = new Mock<TraceFileReader>() { CallBase = true };
			var sut = new TraceManager(null, null, traceFileReaderMock.Object, new Configuration(), loggerMock.Object);
			var filePrefix1 = "7975999f-54d9-4b21-a093-4502ea372723-635497418466831637";
			var filePrefix2 = "7975999f-54d9-4b21-a093-4502ea372723-635497418466831700";
			IEnumerable<Trace> result;

			var clientHeaders = new Dictionary<string, string>()
			{
				{"Content-Type", "text/plain"},
				{"Content-Length", "0"}
			};

			var onPremiseTargetHeaders = new Dictionary<string, string>()
			{
				{"Content-Type", "image/png"},
				{"Content-Length", "500"}
			};

			Directory.CreateDirectory("tracefiles");

			var traceFileWriter = new TraceFileWriter();
			await traceFileWriter.WriteHeaderFile("tracefiles/" + filePrefix1 + ".ct.headers", clientHeaders);
			await traceFileWriter.WriteHeaderFile("tracefiles/" + filePrefix1 + ".optt.headers", onPremiseTargetHeaders);
			await traceFileWriter.WriteHeaderFile("tracefiles/" + filePrefix2 + ".crxxxxxxx.headers", clientHeaders);
			await traceFileWriter.WriteHeaderFile("tracefiles/" + filePrefix2 + ".ltrxxxxxxx.headers", onPremiseTargetHeaders);

			loggerMock.Setup(l => l.Warn(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()));

			result = await sut.GetTracesAsync(Guid.Parse("7975999f-54d9-4b21-a093-4502ea372723"));

			Directory.Delete("tracefiles", true);

			loggerMock.VerifyAll();
			result.Count().Should().Be(1);
		}
	}
}
