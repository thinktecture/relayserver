using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Serilog;
using Thinktecture.Relay.Server.Helper;
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
			return new OnPremiseRequestBuilder(_loggerMock.Object, new ConfigurationDummy());
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

			result = await sut.BuildFromHttpRequest(request, new Guid("276b39f9-f0be-42b7-bcc1-1c2a24289689"), "Google/services/");

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
		public void CombineMultipleHttpHeaderValuesIntoOneCommaSeparatedValue_combines_multiple_HTTP_header_values_into_one()
		{
			var headerValues = new List<string> { "Foo", "Bar", "Baz" };
			var sut = CreateBuilder();
			var result = sut.CombineMultipleHttpHeaderValuesIntoOneCommaSeparatedValue(headerValues);

			result.Should().Be("Foo, Bar, Baz");
		}
	}
}
