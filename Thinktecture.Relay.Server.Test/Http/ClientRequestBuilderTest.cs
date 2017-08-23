using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Http
{
	// ReSharper disable JoinDeclarationAndInitializer
	[TestClass]
	public class ClientRequestBuilderTest
	{
		private readonly Mock<ILogger> _loggerMock;

		public ClientRequestBuilderTest()
		{
			_loggerMock = new Mock<ILogger>();
		}

		private OnPremiseRequestBuilder CreateBuilder()
		{
			return new OnPremiseRequestBuilder(_loggerMock.Object);
		}

		[TestMethod]
		public async Task BuildFrom_correctly_builds_a_ClientRequest_from_given_information()
		{
			var request = new HttpRequestMessage
			{
				Content = new ByteArrayContent(new byte[] { 0, 0, 0 }),
				Method = HttpMethod.Get,
				RequestUri = new Uri("http://tt.invalid/?id=bla")
			};
			var startTime = DateTime.UtcNow;
			var sut = CreateBuilder();
			IOnPremiseConnectorRequest result;

			request.Headers.Host = "tt.invalid"; // should be discarded
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			request.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");

			result = await sut.BuildFrom(request, new Guid("276b39f9-f0be-42b7-bcc1-1c2a24289689"), "Google/services/");

			result.OriginId.Should().Be("276b39f9-f0be-42b7-bcc1-1c2a24289689");
			result.Body.LongLength.Should().Be(3L);
			result.HttpMethod.Should().Be("GET");
			result.RequestId.Should().NotBeNullOrEmpty();
			result.Url.Should().Be("Google/services/?id=bla");
			result.RequestStarted.Should().BeOnOrAfter(startTime).And.BeOnOrBefore(DateTime.UtcNow);
			result.HttpHeaders.Keys.Should().NotContain("Host");
			result.HttpHeaders["Accept"].Should().Be("application/json");
			result.HttpHeaders["Content-Disposition"].Should().Be("attachment");
		}

		[TestMethod]
		public async Task GetClientRequestBodyAsync_returns_null_when_request_content_has_a_length_of_zero()
		{
			var request = new HttpRequestMessage
			{
				Content = new ByteArrayContent(new byte[] { })
			};
			var sut = CreateBuilder();
			byte[] result;

			result = await sut.GetClientRequestBodyAsync(request.Content);

			result.Should().BeNull();
		}

		[TestMethod]
		public async Task GetClientRequestBodyAsync_returns_byte_array_with_request_content_if_length_is_larger_than_zero()
		{
			var request = new HttpRequestMessage
			{
				Content = new ByteArrayContent(new byte[] { 0, 0, 0 })
			};
			var sut = CreateBuilder();
			byte[] result;

			result = await sut.GetClientRequestBodyAsync(request.Content);

			result.LongLength.Should().Be(3L);
		}

		[TestMethod]
		public void CombineMultipleHttpHeaderValuesIntoOneCommaSeperatedValue_combines_multiple_HTTP_header_values_into_one()
		{
			var headerValues = new List<string> { "Foo", "Bar", "Baz" };
			var sut = CreateBuilder();
			string result;

			result = sut.CombineMultipleHttpHeaderValuesIntoOneCommaSeperatedValue(headerValues);

			result.Should().Be("Foo, Bar, Baz");
		}

		[TestMethod]
		public void AddContentHeaders_adds_content_headers_to_ClientRequest_headers()
		{
			var clientRequest = new OnPremiseConnectorRequest
			{
				HttpHeaders = new Dictionary<string, string>
				{
					{"Content-Length", "3"}
				}
			};
			var request = new HttpRequestMessage
			{
				Content = new ByteArrayContent(new byte[] { })
			};
			var sut = CreateBuilder();

			request.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");

			sut.AddContentHeaders(clientRequest, request);

			clientRequest.HttpHeaders.Should().HaveCount(2);
			clientRequest.HttpHeaders.Last().Key.Should().Be("Content-Disposition");
		}

		[TestMethod]
		public void RemoveIgnoredHeaders_removes_ignored_headers_from_ClientRequest()
		{
			var clientRequest = new OnPremiseConnectorRequest
			{
				HttpHeaders = new Dictionary<string, string>
				{
					{"Host", "tt.invalid"},
					{"Connection", "close"},
					{"Content-Length", "3"}
				}
			};
			var sut = CreateBuilder();

			sut.RemoveIgnoredHeaders(clientRequest);

			clientRequest.HttpHeaders.Should().HaveCount(1);
			clientRequest.HttpHeaders.Single().Key.Should().Be("Content-Length");
		}
	}
}
