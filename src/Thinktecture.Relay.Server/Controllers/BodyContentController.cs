using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Thinktecture.Relay.Server.Controllers
{
	/// <summary>
	/// Controller that provides access to stored bodies.
	/// </summary>
	[Authorize(Constants.DefaultAuthenticationPolicy)]
	public class BodyContentController : Controller
	{
		/// <summary>
		/// Streams the body content of the request.
		/// </summary>
		/// <param name="requestId">The unique id of the request.</param>
		/// <param name="bodyStore">An <see cref="IBodyStore"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the <see cref="IActionResult"/>.</returns>
		[Route("body/request")]
		[HttpGet]
		public async Task<IActionResult> GetBodyContentAsync(Guid requestId, [FromServices] IBodyStore bodyStore)
		{
			var stream = await bodyStore.OpenRequestBodyAsync(requestId, HttpContext.RequestAborted);
			Response.RegisterForDisposeAsync(stream);
			return new FileStreamResult(stream, MediaTypeNames.Application.Octet);
		}

		/// <summary>
		/// Stores the body content of the response.
		/// </summary>
		/// <param name="requestId">The unique id of the request.</param>
		/// <param name="bodyStore">An <see cref="IBodyStore"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the amount of bytes written.</returns>
		[Route("body/response")]
		[HttpPost]
		public async Task<IActionResult> StoreBodyContentAsync(Guid requestId, [FromServices] IBodyStore bodyStore)
		{
			var length = await bodyStore.StoreResponseBodyAsync(requestId, Request.Body, HttpContext.RequestAborted);
			return Content(length.ToString(), MediaTypeNames.Text.Plain);
		}
	}
}
