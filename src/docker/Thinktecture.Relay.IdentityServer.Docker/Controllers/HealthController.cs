using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Thinktecture.Relay.IdentityServer.Docker.Controllers;

[AllowAnonymous]
[Route("{controller}/{action}")]
public class HealthController : Controller
{
	[HttpGet]
	public IActionResult Ready()
		=> Ok();

	[HttpGet]
	public IActionResult Check()
		=> Ok();
}
