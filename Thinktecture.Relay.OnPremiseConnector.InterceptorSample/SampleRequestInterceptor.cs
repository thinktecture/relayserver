using System;
using System.IO;
using System.Text;
using Serilog;
using Thinktecture.Relay.OnPremiseConnector.Interceptor;

namespace Thinktecture.Relay.OnPremiseConnector.InterceptorSample
{
	public class SampleRequestInterceptor : IOnPremiseRequestInterceptor
	{
		private readonly ILogger _logger;

		public SampleRequestInterceptor(ILogger logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public void HandleRequest(IInterceptedRequest request)
		{
			_logger.Information("Intercepting request {RequestId} HTTP {Method} {Url}", request.RequestId, request.HttpMethod, request.Url);

			if (request.HttpMethod == "POST" && request.Stream.Length > 0)
			{
				using (var reader = new StreamReader(request.Stream))
				{
					// get original content
					var content = reader.ReadToEnd();
					_logger.Information("Received content {Content}", content);

					// modify content
					content = $"{{ \"modified\": true, \"receivedContent\": {content} }}";
					request.Stream = new MemoryStream(Encoding.UTF8.GetBytes(content)) { Position = 0 };

					// Correct Content-Length header
					var headers = request.CloneHttpHeaders();
					if (headers.ContainsKey("Content-Length"))
					{
						headers["Content-Length"] = request.Stream.Length.ToString();
					}

					request.HttpHeaders = headers;
				}
			}
		}
	}
}
