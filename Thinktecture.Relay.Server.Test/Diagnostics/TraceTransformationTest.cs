using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web.Http.Results;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Thinktecture.Relay.Server.Diagnostics
{
	[TestClass]
	public class TraceTransformationTest
	{
		[TestMethod]
		public void Copies_header_correctly()
		{
			var sut = new TraceTransformation();

			var traceFile = new TraceFile()
			{
				Headers = new Dictionary<string, string>()
				{
					["Content-Type"] = "text/plain",
				}
			};

			var result = sut.CreateFromTraceFile(traceFile);

			result.Content.Headers.ContentLength.Should().Be(0);
			result.Content.Headers.ContentType.ToString().Should().Be("text/plain");
		}

		[TestMethod]
		public void Does_create_content_from_byte_array()
		{
			var sut = new TraceTransformation();

			var traceFile = new TraceFile()
			{
				// Content is abc
				Content = new byte[] { 97, 98, 99 }
			};

			var result = sut.CreateFromTraceFile(traceFile);
			result.Content.Headers.ContentLength.Should().Be(3);
			var content = result.Content.ReadAsStringAsync().Result;
			content.Should().Be("abc");
		}

		[TestMethod]
		public void Does_create_content_from_deflate_byte_array()
		{
			byte[] deflateContent;
			using (var memoryStream = new MemoryStream())
			{
				using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
				using (var writer = new StreamWriter(deflateStream))
				{
					writer.Write("aabbcc");
				}

				deflateContent = memoryStream.ToArray();
			}

			deflateContent.Length.Should().BeGreaterThan(0);

			var sut = new TraceTransformation();

			var traceFile = new TraceFile()
			{
				Content = deflateContent,
				Headers = new Dictionary<string, string>()
				{
					["Content-Encoding"] = "deflate"
				}
			};

			var result = sut.CreateFromTraceFile(traceFile);
			result.Content.Headers.ContentLength.Should().Be(6);
			var content = result.Content.ReadAsStringAsync().Result;
			content.Should().Be("aabbcc");
		}

		[TestMethod]
		public void Does_create_content_from_gzip_byte_array()
		{
			byte[] deflateContent;
			using (var memoryStream = new MemoryStream())
			{
				using (var deflateStream = new GZipStream(memoryStream, CompressionMode.Compress))
				using (var writer = new StreamWriter(deflateStream))
				{
					writer.Write("aabbcc");
				}

				deflateContent = memoryStream.ToArray();
			}

			deflateContent.Length.Should().BeGreaterThan(0);

			var sut = new TraceTransformation();

			var traceFile = new TraceFile()
			{
				Content = deflateContent,
				Headers = new Dictionary<string, string>()
				{
					["Content-Encoding"] = "gzip"
				}
			};

			var result = sut.CreateFromTraceFile(traceFile);
			result.Content.Headers.ContentLength.Should().Be(6);
			var content = result.Content.ReadAsStringAsync().Result;
			content.Should().Be("aabbcc");
		}
	}
}
