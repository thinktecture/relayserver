using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Thinktecture.Relay.Server.Diagnostics
{
	[TestClass]
	public class TraceFileReaderTest
	{
		[TestMethod]
		public async Task Can_read_headers_from_header_file()
		{
			var traceFileWriter = new TraceFileWriter();
			var headers = new Dictionary<string, string>()
			{
				{"Content-Length", "1000"},
				{"Content-Type", "text/plain"}
			};

			await traceFileWriter.WriteHeaderFile("test.header.txt", headers);

			var sut = new TraceFileReader();
			var result = sut.ReadHeaderFileAsync("test.header.txt").Result;

			result.ShouldBeEquivalentTo(headers);

			File.Delete("test.header.txt");
		}

		[TestMethod]
		public void Can_read_empty_content_file()
		{
			File.WriteAllBytes("test.content.txt", new byte[0]);

			var sut = new TraceFileReader();
			var result = sut.ReadContentFileAsync("test.content.txt").Result;

			result.Should().NotBeNull();
			result.Length.Should().Be(0);

			File.Delete("test.content.txt");
		}

		[TestMethod]
		public void Can_read_content_file()
		{
			var bytes = new byte[] { 10, 20, 30 };

			File.WriteAllBytes("test.content.txt", bytes);

			var sut = new TraceFileReader();
			var result = sut.ReadContentFileAsync("test.content.txt").Result;

			result.Should().NotBeNull();
			result.Length.Should().Be(3);
			result.ShouldBeEquivalentTo(bytes);

			File.Delete("test.content.txt");
		}
	}
}
