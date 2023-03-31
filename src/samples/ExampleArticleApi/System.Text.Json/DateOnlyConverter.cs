using System.Text.Json;
using System.Text.Json.Serialization;

namespace ExampleArticleApi.System.Text.Json;
 
/// <summary>
/// Converts a <see cref="DateOnly"/> to and from JSON.
/// </summary>
public class DateOnlyConverter : JsonConverter<DateOnly>
{
	private const string DateFormat = "yyyy-MM-dd";

	/// <inheritdoc />
	public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return DateOnly.ParseExact(reader.GetString() ?? string.Empty, DateFormat);
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString(DateFormat));
	}
}
