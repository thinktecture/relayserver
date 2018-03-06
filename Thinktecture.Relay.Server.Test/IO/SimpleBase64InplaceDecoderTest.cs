using System;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Thinktecture.Relay.Server.IO
{
	[TestClass]
	public class SimpleBase64InplaceDecoderTest
	{
		[TestMethod]
		public void Decode_decodes_base64()
		{
			var data = "This is a Test!";
			var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(data));

			var decoder = new SimpleBase64InplaceDecoder();
			var buffer = Encoding.UTF8.GetBytes(base64);
			var length = decoder.Decode(buffer, buffer.Length, out int offset);

			Encoding.UTF8.GetString(buffer, 0, length).Should().Be(data);
		}

		[TestMethod]
		public void Decode_decodes_base64_in_multiple_parts()
		{
			var data = "This is a Test!";
			var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(data));

			var decoder = new SimpleBase64InplaceDecoder();
			var buffer = Encoding.UTF8.GetBytes(base64);

			var first = new byte[10];
			Buffer.BlockCopy(buffer, 0, first, 0, first.Length);

			var length = decoder.Decode(first, first.Length, out int offset);
			var result = Encoding.UTF8.GetString(first, 0, length);

			var remaining = new byte[buffer.Length];
			if (offset != first.Length)
			{
				Buffer.BlockCopy(first, offset, remaining, 0, first.Length - offset);
			}

			offset = first.Length - offset;

			Buffer.BlockCopy(buffer, first.Length, remaining, offset, buffer.Length - 10);

			length = decoder.Decode(remaining, offset + buffer.Length - 10, out offset);

			result += Encoding.UTF8.GetString(remaining, 0, length);

			result.Should().Be(data);
		}
	}
}
