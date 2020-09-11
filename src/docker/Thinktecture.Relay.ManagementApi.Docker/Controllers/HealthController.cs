using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Thinktecture.Relay.ManagementApi.Docker.Controllers
{
	/// <summary>
	/// Provides health information for this API.
	/// </summary>
	[AllowAnonymous]
	[Route("{controller}/{action}")]
	[ApiExplorerSettings(IgnoreApi = true)]
	public class HealthController : Controller
	{
		/// <summary>
		/// Returns the ready state of this api.
		/// </summary>
		/// <returns>An <see cref="IActionResult"/> representing the state of the api.</returns>
		[HttpGet]
		public IActionResult Ready()
		{
			return Ok();
		}

		/// <summary>
		/// Checks the health state of this api.
		/// </summary>
		/// <returns>An <see cref="IActionResult"/> representing the state of the api.</returns>
		[HttpGet]
		public IActionResult Check()
		{
			return Ok();
		}
	}
}
