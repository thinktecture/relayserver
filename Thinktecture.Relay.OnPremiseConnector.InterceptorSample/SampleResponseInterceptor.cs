using System;
using Serilog;
using Thinktecture.Relay.OnPremiseConnector.Interceptor;

namespace Thinktecture.Relay.OnPremiseConnector.InterceptorSample
{
	public class SampleResponseInterceptor : IOnPremiseResponseInterceptor
	{
		private readonly ILogger _logger;

		public SampleResponseInterceptor(ILogger logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public void HandleResponse(IInterceptedRequest request, IInterceptedResponse response)
		{
			_logger.Information("Intercepting response {RequestId}, Status code: {StatusCode}", request.RequestId, response.StatusCode);

			// modify statuscode
			response.StatusCode++;
		}
	}
}
