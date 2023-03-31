using System.Text.Json.Serialization;
using ExampleArticleApi.System.Text.Json;

namespace ExampleArticleApi.Models;

/// <summary>
/// Represents an article. 
/// </summary>
/// <param name="Title">The title of the article</param>
/// <param name="Url">The url to the article</param>
/// <param name="Tags">The tags of the article</param>
/// <param name="Authors">The authors of the article</param>
/// <param name="Date">The publishing date of the article</param>
/// <param name="Language">The language of the article</param>
// ReSharper disable once ClassNeverInstantiated.Global; Justification: Used for deserialization
public record Article(string Title, Uri Url, string[] Tags, string[] Authors, [property: JsonConverter(typeof(DateOnlyConverter))] DateOnly Date, string Language);
