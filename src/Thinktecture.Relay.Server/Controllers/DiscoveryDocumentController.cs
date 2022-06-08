using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Server.Services;

namespace Thinktecture.Relay.Server.Controllers;

/// <summary>
/// Controller that handles requests to the discovery document.
/// </summary>
[AllowAnonymous]
[Route(DiscoveryDocument.WellKnownPath)]
public partial class DiscoveryDocumentController : Controller
{
	private readonly ILogger<DiscoveryDocumentController> _logger;

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscoveryDocumentController"/> class.
	/// </summary>
	/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
	public DiscoveryDocumentController(ILogger<DiscoveryDocumentController> logger)
		=> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

	[LoggerMessage(20300, LogLevel.Debug, "Returning discovery document")]
	partial void LogReturnDiscoveryDocument();

	/// <summary>
	/// Returns the discovery document that provides initial configuration to the connectors.
	/// </summary>
	/// <param name="documentBuilder">The <see cref="DiscoveryDocumentBuilder"/> that builds the document.</param>
	/// <returns>An object that holds the configuration information.</returns>
	[HttpGet]
	public IActionResult GetDiscoveryDocument([FromServices] DiscoveryDocumentBuilder documentBuilder)
	{
		LogReturnDiscoveryDocument();
		return Ok(documentBuilder.BuildDiscoveryDocument(Request));
	}
}
