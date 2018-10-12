using System.Text;

namespace Thinktecture.Relay.Server.IO
{
	internal class SimpleBase64InplaceDecoder
	{
		private const byte _FILL_BYTE = (byte)'=';

		private static readonly byte[] _base64 = Encoding.UTF8.GetBytes("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/");
		private static readonly byte[] _decode = new byte[256];

		static SimpleBase64InplaceDecoder()
		{
			for (var i = 0; i < _base64.Length; i++)
			{
				_decode[_base64[i]] = (byte)i;
			}
		}

		public int Decode(byte[] buffer, int count, out int offset)
		{
			var index = 0;
			offset = count / 4 * 4;

			for (var i = 0; i < offset; i += 4)
			{
				var triple = _decode[buffer[i]] << 18 | _decode[buffer[i + 1]] << 12 | _decode[buffer[i + 2]] << 6 | _decode[buffer[i + 3]];

				buffer[index++] = (byte)(triple >> 16);

				if (buffer[i + 2] != _FILL_BYTE)
				{
					buffer[index++] = (byte)(triple >> 8);

					if (buffer[i + 3] != _FILL_BYTE)
					{
						buffer[index++] = (byte)triple;
					}
				}
			}

			return index;
		}
	}
}
