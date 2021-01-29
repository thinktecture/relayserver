using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Server.Transport;

namespace Thinktecture.Relay.Server.Controllers
{
	/// <summary>
	/// Controller that provides acknowledgement.
	/// </summary>
	public class AcknowledgeController : Controller
	{
		/// <summary>
		///
		/// </summary>
		/// <param name="originId">The unique id of the origin.</param>
		/// <param name="requestId">The unique id of the request.</param>
		/// <param name="acknowledgeDispatcher">An <see cref="IAcknowledgeDispatcher{T}"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the <see cref="IActionResult"/>.</returns>
		[HttpPost, Route("acknowledge/{originId:guid}/{requestId:guid}")]
		public async Task<IActionResult> AcknowledgeAsync([FromRoute] Guid originId, [FromRoute] Guid requestId,
			[FromServices] IAcknowledgeDispatcher<AcknowledgeRequest> acknowledgeDispatcher)
		{
			await acknowledgeDispatcher.DispatchAsync(new AcknowledgeRequest()
				{ OriginId = originId, RequestId = requestId, RemoveRequestBodyContent = true });

			return NoContent();
		}
	}
}
