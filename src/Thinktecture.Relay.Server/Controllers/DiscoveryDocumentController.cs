using Microsoft.AspNetCore.Mvc;
using Thinktecture.Relay.Server.Services;

namespace Thinktecture.Relay.Server.Controllers
{
	/// <summary>
	/// Controller that handles requests to the discovery document.
	/// </summary>
	[Route(".well-known/relayserver-configuration")]
	public class DiscoveryDocumentController : Controller
	{
		/// <summary>
		/// Returns the RelayServer discovery document that provides initial configuration to the connectors.
		/// </summary>
		/// <param name="documentBuilder">The <see cref="DiscoveryDocumentBuilder"/> that builds the document.</param>
		/// <returns>An object that holds the configuration information.</returns>
		[HttpGet]
		public IActionResult GetDiscoveryDocument([FromServices] DiscoveryDocumentBuilder documentBuilder)
			=> Ok(documentBuilder.BuildDiscoveryDocument());
	}
}