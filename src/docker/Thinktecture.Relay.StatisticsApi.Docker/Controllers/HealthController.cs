using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Thinktecture.Relay.StatisticsApi.Docker.Controllers;

/// <summary>
/// Provides health information for this API.
/// </summary>
[AllowAnonymous]
[Route("{controller}/{action}")]
public class HealthController : Controller
{
	/// <summary>
	/// Returns the ready state of this api.
	/// </summary>
	/// <returns>An <see cref="IActionResult"/> representing the state of the api.</returns>
	[HttpGet]
	public IActionResult Ready()
		=> Ok();

	/// <summary>
	/// Checks the health state of this api.
	/// </summary>
	/// <returns>An <see cref="IActionResult"/> representing the state of the api.</returns>
	[HttpGet]
	public IActionResult Check()
		=> Ok();
}
