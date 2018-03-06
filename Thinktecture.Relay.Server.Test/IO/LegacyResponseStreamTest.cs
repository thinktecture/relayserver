using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Thinktecture.Relay.Server.SignalR;

namespace Thinktecture.Relay.Server.IO
{
	[TestClass]
	public class LegacyResponseStreamTest
	{
		private readonly Mock<IPostDataTemporaryStore> _postDataTemporaryStoreMock;

		public LegacyResponseStreamTest()
		{
			_postDataTemporaryStoreMock = new Mock<IPostDataTemporaryStore>();
		}

		private static string CreateString(int length)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
			return new String(Enumerable.Repeat(1, length).Select((_, index) => chars[index % chars.Length]).ToArray());
		}

		[TestMethod]
		public async Task ExtractBodyAsync_extracts_from_a_small_object_with_large_buffer()
		{
			var body = CreateString(64);
			var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(body));
			var @object = new { Property = CreateString(32), Body = base64, End = true };
			var json = JsonConvert.SerializeObject(@object);
			var source = new MemoryStream(Encoding.UTF8.GetBytes(json));
			var target = new MemoryStream();

			_postDataTemporaryStoreMock.Setup(s => s.CreateResponseStream(It.IsAny<string>())).Returns(target);

			var stream = new LegacyResponseStream(source, _postDataTemporaryStoreMock.Object, null);
			await stream.ExtractBodyAsync();

			using (var reader = new StreamReader(stream))
			{
				var result = await reader.ReadToEndAsync();
				result.Should().Be(JsonConvert.SerializeObject(new { @object.Property, Body = "", @object.End }));
			}
			Encoding.UTF8.GetString(target.ToArray()).Should().Be(body);
			_postDataTemporaryStoreMock.VerifyAll();
		}

		[TestMethod]
		public async Task ExtractBodyAsync_extracts_from_a_large_object_with_small_buffer()
		{
			var body = CreateString(32000);
			var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(body));
			var @object = new { Property = CreateString(500), Body = base64, End = true };
			var json = JsonConvert.SerializeObject(@object);
			var source = new MemoryStream(Encoding.UTF8.GetBytes(json));
			var target = new MemoryStream();

			_postDataTemporaryStoreMock.Setup(s => s.CreateResponseStream(It.IsAny<string>())).Returns(target);

			var stream = new LegacyResponseStream(source, _postDataTemporaryStoreMock.Object, null);
			await stream.ExtractBodyAsync();

			using (var reader = new StreamReader(stream))
			{
				var result = await reader.ReadToEndAsync();
				result.Should().Be(JsonConvert.SerializeObject(new { @object.Property, Body = "", @object.End }));
			}
			Encoding.UTF8.GetString(target.ToArray()).Should().Be(body);
			_postDataTemporaryStoreMock.VerifyAll();
		}
	}
}
