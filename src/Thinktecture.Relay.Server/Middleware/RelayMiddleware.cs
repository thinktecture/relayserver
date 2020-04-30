using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Abstractions;

namespace Thinktecture.Relay.Server.Middleware
{
	// ReSharper disable once ClassNeverInstantiated.Global
	/// <inheritdoc />
	public class RelayMiddleware<TRequest> : IMiddleware
		where TRequest : ITransportClientRequest, new()
	{
		private readonly ITransportClientRequestFactory<TRequest> _requestFactory;
		private readonly ILogger<RelayMiddleware<TRequest>> _logger;

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayMiddleware{TRequest}"/>.
		/// </summary>
		public RelayMiddleware(ITransportClientRequestFactory<TRequest> requestFactory, ILogger<RelayMiddleware<TRequest>> logger)
		{
			_requestFactory = requestFactory;
			_logger = logger;
		}

		/// <inheritdoc />
		public async Task InvokeAsync(HttpContext context, RequestDelegate next)
		{
			var parts = context.Request.Path.Value.Split('/');
			// TODO target can be an empty string
			if (parts.Length >= 3)
			{
				var tenantName = parts[1];
				// TODO verify tenant existence

				context.Request.EnableBuffering();
				await context.Request.Body.DrainAsync(context.RequestAborted);

				Console.WriteLine(context.Request.Body.Length);

				var request = _requestFactory.Create(context);
				_logger?.LogTrace("Parsed request into {@ClientRequest}", request);
			}
			else
			{
				_logger?.LogWarning("Invalid request received {Path}", context.Request.Path);
			}

			await next.Invoke(context);
		}
	}
}
