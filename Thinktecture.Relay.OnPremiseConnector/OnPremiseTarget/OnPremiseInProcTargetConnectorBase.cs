using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

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
			_logger = logger;
		}

		protected abstract IOnPremiseInProcHandler CreateHandler();

		public async Task<IOnPremiseTargetResponse> GetResponseFromLocalTargetAsync(string url, IOnPremiseTargetRequest request, string relayedRequestHeader)
		{
			if (url == null)
				throw new ArgumentNullException(nameof(url));
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			_logger?.Verbose("Requesting response from on-premise in-proc target. request-id={0}, url={1}, origin-id={2}", request.RequestId, url, request.OriginId);

			var response = new OnPremiseTargetResponse()
			{
				RequestId = request.RequestId,
				OriginId = request.OriginId,
				RequestStarted = DateTime.UtcNow,
				HttpHeaders = new Dictionary<string, string>(),
			};

			try
			{
				var handler = CreateHandler();

				try
				{
					using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_requestTimeout)))
					{
						await handler.ProcessRequest(request, response, cts.Token).ConfigureAwait(false);

						if (cts.IsCancellationRequested)
						{
							_logger?.Warning("Gateway timeout");
							_logger?.Verbose("Gateway timeout. request-id={0}", request.RequestId);

							response.StatusCode = HttpStatusCode.GatewayTimeout;
							response.HttpHeaders = new Dictionary<string, string> { ["X-TTRELAY-TIMEOUT"] = "On-Premise Target" };
						}
					}
				}
				catch (Exception ex)
				{
					_logger?.Verbose(ex, "Error requesting response from in-proc target. request-id={0}", request.RequestId);

					response.StatusCode = HttpStatusCode.InternalServerError;
					response.HttpHeaders = new Dictionary<string, string> { ["Content-Type"] = "text/plain" };
					response.Stream = new MemoryStream(Encoding.UTF8.GetBytes(ex.ToString()));
				}
				finally
				{
					// ReSharper disable once SuspiciousTypeConversion.Global
					(handler as IDisposable)?.Dispose();
				}
			}
			catch (Exception ex)
			{
				_logger?.Verbose(ex, "Error creating in-proc handler. request-id={0}", request.RequestId);

				response.StatusCode = HttpStatusCode.InternalServerError;
				response.HttpHeaders = new Dictionary<string, string> { ["Content-Type"] = "text/plain" };
				response.Stream = new MemoryStream(Encoding.UTF8.GetBytes(ex.ToString()));
			}

			response.RequestFinished = DateTime.UtcNow;

			_logger?.Verbose("Got in-proc response. request-id={0}, status-code={1}", response.RequestId, response.StatusCode);

			return response;
		}
	}
}
