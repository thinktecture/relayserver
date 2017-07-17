using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
using Thinktecture.Relay.Server.Dto;

namespace Thinktecture.Relay.Server.Http
{
	// ReSharper disable JoinDeclarationAndInitializer
	// ReSharper disable UnusedAutoPropertyAccessor.Local
	[TestClass]
	public class HttpResponseMessageBuilderTest
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
		public void GetResponseMessage_returns_on_premise_timeout_message_when_given_OnPremiseTargetResponse_is_null()
		{
			IHttpResponseMessageBuilder sut = new HttpResponseMessageBuilder();
			HttpResponseMessage result;

			result = sut.BuildFrom(null, null);

			result.StatusCode.Should().Be(HttpStatusCode.GatewayTimeout);
			result.Content.Headers.Count().Should().Be(1);
			result.Content.Headers.Single().Key.Should().Be("X-TTRELAY-TIMEOUT");
			result.Content.Headers.Single().Value.First().Should().Be("On-Premise");
		}

		[TestMethod]
		public async Task GetResponseMessage_returns_an_HttpResponseMessage_when_given_OnPremiseTargetResponse_is_null()
		{
			IHttpResponseMessageBuilder sut = new HttpResponseMessageBuilder();
			var onPremiseTargetRequest = new OnPremiseTargetResponse
			{
				StatusCode = HttpStatusCode.NotFound,
				Body = new byte[] { 0, 0, 0, 0 },
				HttpHeaders = new Dictionary<string, string>
				{
					{"Content-Length", "4"},
					{"X-Foo", "X-Bar"}
				}
			};
			var link = new Link();
			HttpResponseMessage result;

			result = sut.BuildFrom(onPremiseTargetRequest, link);

			var content = await result.Content.ReadAsByteArrayAsync();
			result.StatusCode.Should().Be(HttpStatusCode.NotFound);
			content.LongLength.Should().Be(4L);
			result.Content.Headers.ContentLength.Should().Be(4L);
			result.Content.Headers.GetValues("X-Foo").Single().Should().Be("X-Bar");
		}

		[TestMethod]
		public void SetHttpHeaders_does_nothing_when_no_HttpHeaders_are_provided()
		{
			var sut = new HttpResponseMessageBuilder();
			var httpContent = new ByteArrayContent(new byte[] { });

			sut.SetHttpHeaders(null, httpContent);

			httpContent.Headers.Should().BeEmpty();
		}

		[TestMethod]
		public void SetHttpHeaders_transforms_HTTP_headers_correctly_from_a_given_OnPremiseTargetRequest_and_sets_them_on_a_given_HttpContent()
		{
			var sut = new HttpResponseMessageBuilder();
			var httpHeaders = new Dictionary<string, string>
			{
				{"Content-MD5", "Q2hlY2sgSW50ZWdyaXR5IQ=="}, // will be discarded
				{"Content-Range", "bytes 21010-47021/47022"}, // will be discarded
				{"Content-Disposition", "attachment"},
				{"Content-Length", "300"},
				{"Content-Location", "http://tt.invalid"},
				{"Content-Type", "application/json"},
				{"Expires", "Thu, 01 Dec 1994 16:00:00 GMT"},
				{"Last-Modified", "Thu, 01 Dec 1994 15:00:00 GMT"}
			};
			var httpContent = new ByteArrayContent(new byte[] { });

			sut.SetHttpHeaders(httpHeaders, httpContent);

			httpContent.Headers.Should().HaveCount(6);
			httpContent.Headers.ContentMD5.Should().BeNull();
			httpContent.Headers.ContentRange.Should().BeNull();
			httpContent.Headers.ContentDisposition.Should().Be(ContentDispositionHeaderValue.Parse("attachment"));
			httpContent.Headers.ContentLength.Should().Be(300L);
			httpContent.Headers.ContentLocation.Should().Be("http://tt.invalid");
			httpContent.Headers.ContentType.Should().Be(MediaTypeHeaderValue.Parse("application/json"));
			httpContent.Headers.Expires.Should().Be(new DateTime(1994, 12, 1, 16, 0, 0));
			httpContent.Headers.LastModified.Should().Be(new DateTime(1994, 12, 1, 15, 0, 0));
		}

		[TestMethod]
		public void SetHttpHeaders_sets_unknown_HTTP_headers_without_validation_correctly_from_a_OnPremiseTargetRequest_on_a_given_HttpContent()
		{
			var sut = new HttpResponseMessageBuilder();
			var httpHeaders = new Dictionary<string, string>
			{
				{"X-Foo", "X-Bar"},
				{"Foo", "Bar"}
			};
			var httpContent = new ByteArrayContent(new byte[] { });

			sut.SetHttpHeaders(httpHeaders, httpContent);

			httpContent.Headers.Should().HaveCount(2);
			httpContent.Headers.GetValues("X-Foo").Single().Should().Be("X-Bar");
			httpContent.Headers.GetValues("Foo").Single().Should().Be("Bar");
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void GetResponseContentForOnPremiseTargetResponse_throws_an_exception_when_given_OnPremiseTargetResponse_is_null()
		{
			var sut = new HttpResponseMessageBuilder();

			sut.GetResponseContentForOnPremiseTargetResponse(null, null);
		}

		[TestMethod]
		public void GetResponseContentForOnPremiseTargetResponse_does_not_disclose_content_when_InternalServerError_occurred_and_ForwardOnPremiseTargetErrorResponse_is_turned_off()
		{
			var sut = new HttpResponseMessageBuilder();
			var onPremiseTargetResponse = new OnPremiseTargetResponse { StatusCode = HttpStatusCode.InternalServerError };
			var link = new Link();
			HttpContent result;

			result = sut.GetResponseContentForOnPremiseTargetResponse(onPremiseTargetResponse, link);

			result.Should().BeNull();
		}

		[TestMethod]
		public async Task GetResponseContentForOnPremiseTargetResponse_discloses_content_when_InternalServerError_occurred_and_ForwardOnPremiseTargetErrorResponse_is_turned_on()
		{
			var sut = new HttpResponseMessageBuilder();
			var onPremiseTargetResponse = new OnPremiseTargetResponse { StatusCode = HttpStatusCode.InternalServerError, Body = new byte[] { 0, 0, 0 } };
			var link = new Link { ForwardOnPremiseTargetErrorResponse = true };
			HttpContent result;

			result = sut.GetResponseContentForOnPremiseTargetResponse(onPremiseTargetResponse, link);

			var body = await result.ReadAsByteArrayAsync();
			result.Should().NotBeNull();
			body.LongLength.Should().Be(3L);
		}

		[TestMethod]
		public async Task GetResponseContentForOnPremiseTargetResponse_sets_StatusCode_accordingly_and_discloses_content()
		{
			var sut = new HttpResponseMessageBuilder();
			var onPremiseTargetResponse = new OnPremiseTargetResponse { StatusCode = HttpStatusCode.OK, Body = new byte[] { 0, 0, 0, 0 } };
			var link = new Link();
			HttpContent result;

			result = sut.GetResponseContentForOnPremiseTargetResponse(onPremiseTargetResponse, link);

			var body = await result.ReadAsByteArrayAsync();
			result.Should().NotBeNull();
			body.LongLength.Should().Be(4L);
		}
	}
}