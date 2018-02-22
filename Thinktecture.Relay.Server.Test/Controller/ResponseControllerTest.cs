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

namespace Thinktecture.Relay.Server.Controller
{
	[TestClass]
	public class ResponseControllerTest
	{
		private readonly Mock<IPostDataTemporaryStore> _postDataTemporaryStoreMock;

		public ResponseControllerTest()
		{
			_postDataTemporaryStoreMock = new Mock<IPostDataTemporaryStore>();
		}

		private static Random random = new Random();

		private static string CreateString(int length)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
			return new String(Enumerable.Repeat(1, length).Select((_, index) => chars[index % chars.Length]).ToArray());
		}

		[TestMethod]
		public void LegacyResponseStream_ExtractBodyAsync_extracts_from_a_small_object_with_large_buffer()
		{
			var body = CreateString(64);
			var @object = new { Property = CreateString(32), Body = Convert.ToBase64String(Encoding.ASCII.GetBytes(body)), End = true };
			var json = JsonConvert.SerializeObject(@object);
			var source = new MemoryStream(Encoding.ASCII.GetBytes(json));
			var target = new MemoryStream();

			_postDataTemporaryStoreMock.Setup(s => s.CreateResponseStream(It.IsAny<string>())).Returns(target);

			var reader = new ResponseController.LegacyResponseReader(source, Encoding.UTF8, _postDataTemporaryStoreMock.Object, null);

			var result = "";
			while (true)
			{
				char[] buffer = new char[0x1000];
				var length = reader.Read(buffer, 0, buffer.Length);
				if (length == 0)
				{
					break;
				}

				result += new String(buffer, 0, length);
			}

			result.Should().Be(JsonConvert.SerializeObject(new { @object.Property, Body = "", @object.End }));
			Encoding.UTF8.GetString(target.ToArray()).Should().Be(body);
			_postDataTemporaryStoreMock.VerifyAll();
		}

		[TestMethod]
		public void LegacyResponseStream_ExtractBodyAsync_extracts_from_a_large_object_with_small_buffer()
		{
			var body = CreateString(512);
			var @object = new { Property = CreateString(256), Body = Convert.ToBase64String(Encoding.ASCII.GetBytes(body)), End = true };
			var json = JsonConvert.SerializeObject(@object);
			var source = new MemoryStream(Encoding.ASCII.GetBytes(json));
			var target = new MemoryStream();

			_postDataTemporaryStoreMock.Setup(s => s.CreateResponseStream(It.IsAny<string>())).Returns(target);

			var reader = new ResponseController.LegacyResponseReader(source, Encoding.UTF8, _postDataTemporaryStoreMock.Object, null, 256);

			var result = "";
			while (true)
			{
				char[] buffer = new char[64];
				var length = reader.Read(buffer, 0, buffer.Length);
				if (length == 0)
				{
					break;
				}

				result += new String(buffer, 0, length);
			}

			result.Should().Be(JsonConvert.SerializeObject(new { @object.Property, Body = "", @object.End }));
			Encoding.UTF8.GetString(target.ToArray()).Should().Be(body);
			_postDataTemporaryStoreMock.VerifyAll();
		}
	}
}
