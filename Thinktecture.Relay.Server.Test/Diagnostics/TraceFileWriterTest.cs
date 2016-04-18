using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Thinktecture.Relay.Server.Diagnostics
{
	[TestClass]
	public class TraceFileWriterTest
	{
		// ReSharper disable JoinDeclarationAndInitializer
		[TestMethod]
		public void WriteHeaderFile_writes_a_file_to_disk_that_contains_the_headers_as_json()
		{
			ITraceFileWriter sut = new TraceFileWriter();
			IDictionary<string, string> headers = new Dictionary<string, string>
			{
				{"Content-Disposition", "attachment"},
				{"Content-Length", "3"},
				{"X-TTRELAY-TIMEOUT", "OnPremise"},
			};

			sut.WriteHeaderFile("test.headers.txt", headers);

			Thread.Sleep(100); // File write is async

            var referenceObject = JsonConvert.DeserializeObject<IDictionary<string, string>>(File.ReadAllText("test.headers.txt", Encoding.UTF8));

		    referenceObject.ShouldBeEquivalentTo(headers);

			File.Delete("test.headers.txt");
		}

		[TestMethod]
		public void WriteContentFile_writes_a_file_to_disk_that_contains_the_content()
		{
			ITraceFileWriter sut = new TraceFileWriter();
			var content = new byte[] { 65, 66, 67 };

			sut.WriteContentFile("test.content.txt", content);

			Thread.Sleep(100); // File write is async

			File.ReadAllText("test.content.txt", Encoding.ASCII).Should().Be("ABC");

			File.Delete("test.content.txt");
		}

		[TestMethod]
		public void WriteContentFile_writes_an_empty_file_to_disk_if_content_is_null()
		{
			ITraceFileWriter sut = new TraceFileWriter();
			var content = new byte[] { 65, 66, 67 };

			sut.WriteContentFile("test.content.txt", null);

			Thread.Sleep(100); // File write is async

			File.ReadAllText("test.content.txt", Encoding.ASCII).Should().BeEmpty();

			File.Delete("test.content.txt");
		}
	}
}
