using System;
using System.IO;
using System.Net.Mime;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Server.Endpoints;

internal static partial class EndpointRouteBuilderExtensions
{
	public static IEndpointRouteBuilder MapBodyContentEndpoints(this IEndpointRouteBuilder app, string pattern)
	{
		app.MapGet($"{pattern}/request/{{requestId:guid}}", BodyContentEndpoint.GetRequestBodyContentAsync)
			.WithName("GetRequestBodyContent")
			.WithDisplayName("Get request body content")
			.Produces(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status401Unauthorized)
			.Produces(StatusCodes.Status403Forbidden)
			.RequireAuthorization(Constants.DefaultAuthenticationPolicy);

		app.MapPost($"{pattern}/response/{{requestId:guid}}", BodyContentEndpoint.StoreResponseBodyContentAsync)
			.WithName("StoreResponseBodyContent")
			.WithDisplayName("Store response body content")
			.Produces(StatusCodes.Status204NoContent)
			.Produces(StatusCodes.Status401Unauthorized)
			.Produces(StatusCodes.Status403Forbidden)
			.RequireAuthorization(Constants.DefaultAuthenticationPolicy);

		return app;
	}
}

internal partial class BodyContentEndpoint
{
	public static async Task<IResult> GetRequestBodyContentAsync([FromServices] ILogger<BodyContentEndpoint> logger,
		[FromRoute] Guid requestId, [FromServices] IBodyStore bodyStore,
		[FromServices] IHttpContextAccessor httpContextAccessor, [FromQuery] bool delete = false,
		CancellationToken cancellationToken = default)
	{
		Log.DeliverBody(logger, requestId, delete);

		var stream = await bodyStore.OpenRequestBodyAsync(requestId, cancellationToken);
		var response = Results.Stream(stream, MediaTypeNames.Application.Octet);

		if (delete)
		{
			httpContextAccessor.HttpContext!.Response.RegisterForDisposeAsync(
				bodyStore.GetRequestBodyRemoveDisposable(requestId));
		}

		return response;
	}

	public static async Task<IResult> StoreResponseBodyContentAsync([FromServices] ILogger<BodyContentEndpoint> logger,
		[FromRoute] Guid requestId, [FromServices] IBodyStore bodyStore, [FromBody] Stream body,
		[FromServices] IHttpContextAccessor httpContextAccessor, CancellationToken cancellationToken = default)
	{
		Log.StoreBody(logger, requestId);

		try
		{
			var length = await bodyStore.StoreResponseBodyAsync(requestId, body, cancellationToken);
			return Results.Text(length.ToString(), MediaTypeNames.Text.Plain);
		}
		catch (OperationCanceledException)
		{
			Log.ResponseAborted(logger, httpContextAccessor.HttpContext!.User.GetTenantName(), requestId);
			return Results.NoContent();
		}
	}
}
