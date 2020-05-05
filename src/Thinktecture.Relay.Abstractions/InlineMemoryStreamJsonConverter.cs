using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Thinktecture.Relay
{
	internal class InlineMemoryStreamJsonConverter : JsonConverter<Stream>
	{
		public override Stream Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			=> new MemoryStream(Convert.FromBase64String(reader.GetString()));

		public override void Write(Utf8JsonWriter writer, Stream value, JsonSerializerOptions options)
		{
			if (value is MemoryStream stream)
			{
				writer.WriteBase64StringValue(stream.GetBuffer());
				return;
			}

			writer.WriteNullValue();
		}
	}
}
