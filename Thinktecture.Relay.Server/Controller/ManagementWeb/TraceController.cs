using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.Relay.Server.Diagnostics;
using Thinktecture.Relay.Server.Dto;
using Thinktecture.Relay.Server.Repository;

namespace Thinktecture.Relay.Server.Controller.ManagementWeb
{
	[Authorize(Roles = "Admin")]
	[ManagementWebModuleBindingFilter]
	public class TraceController : ApiController
	{
		private readonly ITraceRepository _traceRepository;
		private readonly ITraceManager _traceManager;
		private readonly ITraceTransformation _traceTransformation;

		public TraceController(ITraceRepository traceRepository, ITraceManager traceManager, ITraceTransformation traceTransformation)
		{
			_traceRepository = traceRepository;
			_traceManager = traceManager;
			_traceTransformation = traceTransformation;
		}

		[HttpGet]
		[ActionName("traceconfigurations")]
		public IHttpActionResult Get(Guid linkId)
		{
			var result = _traceRepository.GetTraceConfigurations(linkId);

			return Ok(result);
		}

		[HttpGet]
		[ActionName("traceconfiguration")]
		public IHttpActionResult GetTraceConfiguration(Guid traceConfigurationId)
		{
			var result = _traceRepository.GetTraceConfiguration(traceConfigurationId);

			return Ok(result);
		}

		[HttpGet]
		[ActionName("isrunning")]
		public IHttpActionResult IsRunning(Guid linkId)
		{
			var traceConfiguration = _traceRepository.GetRunningTranceConfiguration(linkId);

			var result = new RunningTraceConfiguration()
			{
				IsRunning = traceConfiguration != null,
				TraceConfiguration = traceConfiguration
			};

			return Ok(result);
		}

		[HttpPost]
		[ActionName("traceconfiguration")]
		public IHttpActionResult Create(StartTrace startTrace)
		{
			if (startTrace.Minutes < 1)
			{
				return BadRequest("Tracking must be enabled for one minute at least.");
			}

			if (startTrace.Minutes > 10)
			{
				return BadRequest("Tracking can only be enabled for ten minutes at most.");
			}

			var traceConfiguration = new TraceConfiguration
			{
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMinutes(startTrace.Minutes),
				LinkId = startTrace.LinkId
			};

			_traceRepository.Create(traceConfiguration);

			// TODO: Location
			return Created("", traceConfiguration);
		}

		[HttpDelete]
		[ActionName("traceconfiguration")]
		public IHttpActionResult Disable(Guid traceConfigurationId)
		{
			var disabled = _traceRepository.Disable(traceConfigurationId);

			if (disabled)
			{
				return Ok();
			}

			return BadRequest();
		}

		[HttpGet]
		[ActionName("fileinformations")]
		public async Task<IHttpActionResult> GetFileInformations(Guid traceConfigurationId)
		{
			var result = await _traceManager.GetTracesAsync(traceConfigurationId).ConfigureAwait(false);

			return Ok(result);
		}

		[HttpGet]
		[ActionName("view")]
		public async Task<HttpResponseMessage> ViewAsync(string headerFileName)
		{
			var trace = await _traceManager.GetTraceFileAsync(headerFileName).ConfigureAwait(false);

			var result = _traceTransformation.CreateFromTraceFile(trace);

			return result;
		}

		// TODO: Temporary token needed, currently disabled
		//[HttpGet]
		//[ActionName("download")]
		//public async Task<HttpResponseMessage> Download(string url)
		//{
		//    var httpClient = new HttpClient();
		//    var decodedUrl = HttpUtility.UrlDecode(url);

		//    // TODO: Replace this very simple example of getting the extension
		//    var extension = Path.GetExtension(decodedUrl);
		//    var httpResult = await httpClient.GetAsync(decodedUrl);

		//    if (httpResult.Content.Headers.Contains("Content-Type"))
		//    {
		//        httpResult.Content.Headers.Remove("Content-Type");
		//    }

		//    httpResult.Content.Headers.Add("Content-Type", "application/octet-stream");
		//    httpResult.Content.Headers.Add("Content-Disposition", String.Format("attachment;filename=\"download{0}\"", extension));

		//    return httpResult;
		//}
	}
}
