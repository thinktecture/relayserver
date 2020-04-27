using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Thinktecture.Relay.Server.Docker.Controllers
{
	[AllowAnonymous]
	[Route("{controller}/{action}")]
	public class HealthController : Controller
	{
		[HttpGet]
		public IActionResult Ready()
		{
			return Ok();
		}

		[HttpGet]
		public IActionResult Check()
		{
			return Ok();
		}
	}
}
