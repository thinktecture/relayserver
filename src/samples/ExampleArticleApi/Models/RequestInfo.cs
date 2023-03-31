namespace ExampleArticleApi.Models;

/// <summary>
/// Represents information about a request.
/// </summary>
/// <param name="Time">The Time of the request.</param>
/// <param name="Method">The method of the request.</param>
/// <param name="Path">The path of the request.</param>
/// <param name="Query">The query string of the request.</param>
/// <param name="Headers">The headers of the request.</param>
/// <param name="IpAddress">The ip address of the requesting host.</param>
public record RequestInfo(DateTimeOffset Time, string Method, string Path, string Query, string[] Headers, string IpAddress);
