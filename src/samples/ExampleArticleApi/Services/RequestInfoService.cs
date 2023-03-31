using System.Collections.Concurrent;
using ExampleArticleApi.Models;

namespace ExampleArticleApi.Services;

/// <summary>
/// Represents a service to keep track of requests.
/// </summary>
public class RequestInfoService
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly ConcurrentBag<RequestInfo> _data = new();

	/// <summary>
	/// Creates a new instance of the <see cref="RequestInfoService"/> class.
	/// </summary>
	/// <param name="httpContextAccessor">An instance of an <see cref="IHttpContextAccessor"/> object.</param>
	public RequestInfoService(IHttpContextAccessor httpContextAccessor)
		=> _httpContextAccessor = httpContextAccessor;

	
	/// <summary>
	/// Logs the current request.
	/// </summary>
	public void LogRequest()
	{
		var context = _httpContextAccessor.HttpContext ??
			throw new InvalidOperationException("Need a http context to log the request");

		_data.Add(new RequestInfo(
			DateTimeOffset.Now,
			context.Request.Method,
			context.Request.Path,
			context.Request.QueryString.ToString(),
			context.Request.Headers.Select(kvp => $"{kvp.Key}: {String.Join("; ", kvp.Value)}").ToArray(),
			context.Connection.RemoteIpAddress?.ToString() ?? string.Empty
		));
	}

	/// <summary>
	/// Gets all logged requests.
	/// </summary>
	public IEnumerable<RequestInfo> GetRequests => _data.OrderByDescending(r => r.Time);
}
