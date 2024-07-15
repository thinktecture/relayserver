using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Server.Transport;

namespace Thinktecture.Relay.Server.Endpoints;

internal static partial class EndpointRouteBuilderExtensions
{
	public static IEndpointRouteBuilder MapAcknowledge(this IEndpointRouteBuilder app, string pattern)
	{
		app.MapPost($"{pattern}/{{originId:guid}}/{{requestId:guid}}", AcknowledgeEndpoint.HandleRequestAsync)
			.WithName("Acknowledge")
			.WithDisplayName("Acknowledge")
			.Produces(StatusCodes.Status204NoContent);

		return app;
	}
}

internal partial class AcknowledgeEndpoint
{
	public static async Task<IResult> HandleRequestAsync([FromServices] ILogger<AcknowledgeEndpoint> logger,
		[FromRoute] Guid originId, [FromRoute] Guid requestId,
		[FromServices] IAcknowledgeDispatcher<AcknowledgeRequest> acknowledgeDispatcher,
		CancellationToken cancellationToken = default)
	{
		Log.AcknowledgementReceived(logger, requestId, originId);

		await acknowledgeDispatcher.DispatchAsync(new AcknowledgeRequest
		{
			OriginId = originId,
			RequestId = requestId,
			RemoveRequestBodyContent = true
		}, cancellationToken);

		return Results.NoContent();
	}
}
