using ExampleArticleApi.Models;
using ExampleArticleApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExampleArticleApi.Controllers;

/// <summary>
/// Handles http requests related to requests.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RequestsController : ControllerBase
{
	private readonly RequestInfoService _requestInfoService;

	/// <summary>
	/// Creates a new instance of <see cref="RequestsController"/>.
	/// </summary>
	/// <param name="requestInfoService">An instance of an <see cref="RequestInfoService"/> object.</param>
	public RequestsController(RequestInfoService requestInfoService)
		=> _requestInfoService = requestInfoService;

	/// <summary>
	/// Gets all requests.
	/// </summary>
	/// <returns>All requests that have hit the other endpoints.</returns>
	[HttpGet]
	public ActionResult<IEnumerable<RequestInfo>> Get()
	{
		return Ok(_requestInfoService.GetRequests);
	}
}
