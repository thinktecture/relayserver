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
using Thinktecture.Relay.Server.Helper;
using Thinktecture.Relay.Server.OnPremise;
using Thinktecture.Relay.Server.SignalR;

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
			return new OnPremiseRequestBuilder(_loggerMock.Object, new InMemoryPostDataTemporaryStore(_loggerMock.Object, new ConfigurationDummy()));
		}

		[TestMethod]
		public async Task BuildFromHttpRequest_correctly_builds_a_ClientRequest_from_given_information()
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

			result = await sut.BuildFromHttpRequest(request, new Guid("276b39f9-f0be-42b7-bcc1-1c2a24289689"), "Google/services/", "/relay/Foo/Google");

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
		public void CombineMultipleHttpHeaderValuesIntoOneCommaSeperatedValue_combines_multiple_HTTP_header_values_into_one()
		{
			var headerValues = new List<string> { "Foo", "Bar", "Baz" };
			var sut = CreateBuilder();
			var result = sut.CombineMultipleHttpHeaderValuesIntoOneCommaSeperatedValue(headerValues);

			result.Should().Be("Foo, Bar, Baz");
		}

		[TestMethod]
		public async Task BuildFromHttpRequest_correctly_adds_forwarded_header_with_standard_port()
		{
			// arrange
			var sut = CreateBuilder();
			var request = new HttpRequestMessage
			{
				Method = HttpMethod.Get,
				RequestUri = new Uri("https://tt.invalid/relay/tenantusername/targetname/local/url?id=bla"),
				Content = new ByteArrayContent(new byte[] { 0, 0, 0 })
			};

			// act
			var result = await sut.BuildFromHttpRequest(request, new Guid(), "targetname/local/url?id=bla", "/relay/tenantusername/targetname");

			// assert
			var forwardedParameters = result.HttpHeaders["Forwarded"]
				.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(p => p.Split('='))
				.ToDictionary(kvp => kvp[0].Trim(), kvp => kvp[1].Trim());

			forwardedParameters["proto"].Should().Be("https");
			forwardedParameters["host"].Should().Be("tt.invalid");
			forwardedParameters["path"].Should().Be("/relay/tenantusername/targetname");
		}

		[TestMethod]
		public async Task BuildFromHttpRequest_correctly_adds_forwarded_header_with_non_standard_port()
		{
			// arrange
			var sut = CreateBuilder();
			var request = new HttpRequestMessage
			{
				Method = HttpMethod.Get,
				RequestUri = new Uri("http://tt.invalid:12345/relay/tenantusername/targetname/local/url?id=bla"),
				Content = new ByteArrayContent(new byte[] { 0, 0, 0 })
			};

			// act
			var result = await sut.BuildFromHttpRequest(request, new Guid(), "targetname/local/url?id=bla", "/relay/tenantusername/targetname");

			// assert
			var forwardedParameters = result.HttpHeaders["Forwarded"]
				.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(p => p.Split('='))
				.ToDictionary(kvp => kvp[0].Trim(), kvp => kvp[1].Trim());

			forwardedParameters["proto"].Should().Be("http");
			forwardedParameters["host"].Should().Be("tt.invalid:12345");
			forwardedParameters["path"].Should().Be("/relay/tenantusername/targetname");
		}


		[TestMethod]
		public async Task BuildFromHttpRequest_correctly_adds_forwarded_header_when_request_already_has_a_forwarded_header_without_host_parameter()
		{
			// arrange
			var sut = CreateBuilder();
			var request = new HttpRequestMessage
			{
				Method = HttpMethod.Get,
				RequestUri = new Uri("http://tt.invalid:12345/relay/tenantusername/targetname/local/url?id=bla"),
				Content = new ByteArrayContent(new byte[] { 0, 0, 0 }),
			};
			request.Headers.Add("Forwarded", "for=8.8.8.8, proto=https");

			// act
			var result = await sut.BuildFromHttpRequest(request, new Guid(), "targetname/local/url?id=bla", "/relay/tenantusername/targetname");

			// assert
			var forwardedParameters = result.HttpHeaders["Forwarded"]
				.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(p => p.Split('='))
				.ToDictionary(kvp => kvp[0].Trim(), kvp => kvp[1].Trim());

			forwardedParameters["proto"].Should().Be("http");
			forwardedParameters["host"].Should().Be("tt.invalid:12345");
			forwardedParameters["path"].Should().Be("/relay/tenantusername/targetname");
		}

		[TestMethod]
		public async Task BuildFromHttpRequest_leaves_existing_forwarded_header()
		{
			// arrange
			var sut = CreateBuilder();
			var request = new HttpRequestMessage
			{
				Method = HttpMethod.Get,
				RequestUri = new Uri("http://tt.invalid:12345/relay/tenantusername/targetname/local/url?id=bla"),
				Content = new ByteArrayContent(new byte[] { 0, 0, 0 }),
			};
			request.Headers.Add("Forwarded", "host=tt.cdn,proto=https,path=/test");

			// act
			var result = await sut.BuildFromHttpRequest(request, new Guid(), "targetname/local/url?id=bla", "/relay/tenantusername/targetname");

			// assert
			var forwardedParameters = result.HttpHeaders["Forwarded"]
				.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(p => p.Split('='))
				.ToDictionary(kvp => kvp[0].Trim(), kvp => kvp[1].Trim());

			forwardedParameters["proto"].Should().Be("https");
			forwardedParameters["host"].Should().Be("tt.cdn");
			forwardedParameters["path"].Should().Be("/test");
		}
	}
}
