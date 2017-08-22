using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
	internal abstract class OnPremiseInProcTargetConnectorBase : IOnPremiseTargetConnector
	{
		private readonly int _requestTimeout;
		private readonly ILogger _logger;

		protected OnPremiseInProcTargetConnectorBase(ILogger logger, int requestTimeout)
		{
			if (requestTimeout < 0)
				throw new ArgumentOutOfRangeException(nameof(requestTimeout), "Request timeout cannot be negative.");

			_requestTimeout = requestTimeout;
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		protected abstract IOnPremiseInProcHandler CreateHandler();

		public async Task<IOnPremiseTargetResponse> GetResponseAsync(string url, IOnPremiseTargetRequest request)
		{
			if (url == null)
				throw new ArgumentNullException(nameof(url));
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			_logger.Debug("Requesting response from on-premise in-proc target");
			_logger.Trace("Requesting response from on-premise in-proc target. request-id={0}, url={1}, origin-id={2}", request.RequestId, url, request.OriginId);

			var response = new OnPremiseTargetResponse()
			{
				RequestId = request.RequestId,
				OriginId = request.OriginId,
				RequestStarted = DateTime.UtcNow,
				HttpHeaders = new Dictionary<string, string>()
			};

			try
			{
				var handler = CreateHandler();

				try
				{
					using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_requestTimeout)))
					{
						await handler.ProcessRequest(request, response, cts.Token);

						if (cts.IsCancellationRequested)
						{
							_logger.Warn("Gateway timeout");
							_logger.Trace("Gateway timeout. request-id={0}", request.RequestId);

							response.StatusCode = HttpStatusCode.GatewayTimeout;
							response.HttpHeaders = new Dictionary<string, string>()
							{
								{"X-TTRELAY-TIMEOUT", "On-Premise Target"}
							};
							response.Body = null;
						}
					}
				}
				catch (Exception ex)
				{
					_logger.Trace(ex, "Error requesting response. request-id={0}", request.RequestId);

					response.StatusCode = HttpStatusCode.InternalServerError;
					response.HttpHeaders = new Dictionary<string, string>()
					{
						{"Content-Type", "text/plain"}
					};
					response.Body = Encoding.UTF8.GetBytes(ex.ToString());
				}
				finally
				{
					// ReSharper disable once SuspiciousTypeConversion.Global
					if (handler is IDisposable disposable)
					{
						disposable.Dispose();
					}
				}
			}
			catch (Exception ex)
			{
				_logger.Trace(ex, "Error creating handler. request-id={0}", request.RequestId);

				response.StatusCode = HttpStatusCode.InternalServerError;
				response.HttpHeaders = new Dictionary<string, string>()
				{
					{"Content-Type", "text/plain"}
				};
				response.Body = Encoding.UTF8.GetBytes(ex.ToString());
			}

			response.RequestFinished = DateTime.UtcNow;

			_logger.Trace("Got response. request-id={0}, status-code={1}", response.RequestId, response.StatusCode);

			return response;
		}
	}
}
