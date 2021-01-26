using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Thinktecture.Relay
{
	internal class TimeSpanJsonConverter : JsonConverter<TimeSpan>
	{
		public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			=> TimeSpan.Parse(reader.GetString(), CultureInfo.InvariantCulture);

		public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
			=> writer.WriteStringValue(value.ToString("c"));
	}

	internal class NullableTimeSpanJsonConverter : JsonConverter<TimeSpan?>
	{
		public override TimeSpan? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return reader.TokenType == JsonTokenType.String
				? (TimeSpan?)TimeSpan.Parse(reader.GetString(), CultureInfo.InvariantCulture)
				: null;
		}

		public override void Write(Utf8JsonWriter writer, TimeSpan? value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
			}
			else
			{
				writer.WriteStringValue(value.Value.ToString("c"));
			}
		}
	}
}
