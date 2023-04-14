using System.Text.Json;
using ExampleArticleApi.Models;
using ExampleArticleApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExampleArticleApi.Controllers;

/// <summary>
/// Handles http requests related to articles.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
	private readonly RequestInfoService _requestInfoService;
	private readonly string _dataFileName;

	/// <summary>
	/// Creates a new instance of <see cref="ArticlesController"/>.
	/// </summary>
	/// <param name="configuration">An instance of an <see cref="IConfiguration"/> object.</param>
	/// <param name="requestInfoService">An instance of an <see cref="RequestInfoService"/> object.</param>
	public ArticlesController(IConfiguration configuration, RequestInfoService requestInfoService)
	{
		_requestInfoService = requestInfoService;
		_dataFileName = configuration["Datafile"];
	}

	/// <summary>
	/// Gets all articles.
	/// </summary>
	/// <param name="cancellationToken">A cancellationToken that can be used to abort the current request.</param>
	/// <returns>A list of articles.</returns>
	[HttpGet]
	public async Task<ActionResult<IEnumerable<Article>>> GetAsync(CancellationToken cancellationToken)
	{
		_requestInfoService.LogRequest();

		await using var fs = new FileStream(_dataFileName, FileMode.Open, FileAccess.Read, FileShare.Read, 1024,
			FileOptions.Asynchronous);

		return await JsonSerializer.DeserializeAsync<Article[]>(fs, cancellationToken: cancellationToken)
			?? Array.Empty<Article>();
	}
}
