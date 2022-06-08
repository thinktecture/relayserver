using System;
using System.Net.Mime;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Server.Controllers;

/// <summary>
/// Controller that provides access to stored bodies.
/// </summary>
[Authorize(Constants.DefaultAuthenticationPolicy)]
public partial class BodyContentController : Controller
{
	private readonly ILogger<BodyContentController> _logger;

	/// <summary>
	/// Initializes a new instance of the <see cref="BodyContentController"/> class.
	/// </summary>
	/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
	public BodyContentController(ILogger<BodyContentController> logger)
		=> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

	[LoggerMessage(20200, LogLevel.Debug,
		"Delivering request body content for request {RequestId}, should delete: {DeleteBody}")]
	partial void LogDeliverBody(Guid requestId, bool deleteBody);

	/// <summary>
	/// Streams the body content of the request.
	/// </summary>
	/// <param name="requestId">The unique id of the request.</param>
	/// <param name="bodyStore">An <see cref="IBodyStore"/>.</param>
	/// <param name="delete">Indicates if the element should be deleted at the end of the request.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the <see cref="IActionResult"/>.</returns>
	[HttpGet]
	[Route("body/request/{requestId:guid}")]
	public async Task<IActionResult> GetRequestBodyContentAsync([FromRoute] Guid requestId,
		[FromServices] IBodyStore bodyStore,
		[FromQuery] bool delete = false)
	{
		LogDeliverBody(requestId, delete);

		var stream = await bodyStore.OpenRequestBodyAsync(requestId, HttpContext.RequestAborted);
		Response.RegisterForDisposeAsync(stream);

		if (delete)
		{
			Response.RegisterForDisposeAsync(bodyStore.GetRequestBodyRemoveDisposable(requestId));
		}

		return new FileStreamResult(stream, MediaTypeNames.Application.Octet);
	}

	[LoggerMessage(20201, LogLevel.Debug, "Storing response body content for request {RequestId}")]
	partial void LogStoreBody(Guid requestId);

	/// <summary>
	/// Stores the body content of the response.
	/// </summary>
	/// <param name="requestId">The unique id of the request.</param>
	/// <param name="bodyStore">An <see cref="IBodyStore"/>.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the amount of bytes written.</returns>
	[HttpPost]
	[Route("body/response/{requestId:guid}")]
	public async Task<IActionResult> StoreResponseBodyContentAsync([FromRoute] Guid requestId,
		[FromServices] IBodyStore bodyStore)
	{
		LogStoreBody(requestId);

		try
		{
			var length = await bodyStore.StoreResponseBodyAsync(requestId, Request.Body, HttpContext.RequestAborted);
			return Content(length.ToString(), MediaTypeNames.Text.Plain);
		}
		catch (OperationCanceledException)
		{
			_logger.LogWarning(20202, "Connector for {TenantId} aborted the response body upload for request {RequestId}",
				User.GetTenantInfo().Id, requestId);
			return NoContent();
		}
	}
}
