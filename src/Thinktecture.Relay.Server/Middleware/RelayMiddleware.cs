using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Server.Persistence;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Middleware
{
	/// <inheritdoc />
	public class RelayMiddleware<TRequest> : IMiddleware
		where TRequest : IRelayClientRequest
	{
		private readonly IRelayClientRequestFactory<TRequest> _requestFactory;
		private readonly ILogger<RelayMiddleware<TRequest>> _logger;
		private readonly ITenantRepository _tenantRepository;

		/// <summary>
		/// Initializes a new instance of <see cref="RelayMiddleware{TRequest}"/>.
		/// </summary>
		public RelayMiddleware(IRelayClientRequestFactory<TRequest> requestFactory, ILogger<RelayMiddleware<TRequest>> logger,
			ITenantRepository tenantRepository)
		{
			_requestFactory = requestFactory;
			_logger = logger;
			_tenantRepository = tenantRepository;
		}

		/// <inheritdoc />
		public async Task InvokeAsync(HttpContext context, RequestDelegate next)
		{
			var tenantName = context.Request.Path.Value.Split('/').Skip(1).FirstOrDefault();
			if (!string.IsNullOrEmpty(tenantName))
			{
				var tenant = await _tenantRepository.LoadTenantByNameAsync(tenantName);
				if (tenant == null)
				{
					_logger?.LogWarning("Unknown tenant in request received {Path}{Query}", context.Request.Path, context.Request.QueryString);

					await next.Invoke(context);
					return;
				}

				context.Request.EnableBuffering();
				await context.Request.Body.DrainAsync(context.RequestAborted);

				var request = await _requestFactory.CreateAsync(context, tenant.Id);
				_logger?.LogTrace("Parsed request into {@ClientRequest}", request);

				context.Response.ContentType = "application/json";
				await context.Response.WriteAsync(JsonSerializer.Serialize(request), context.RequestAborted);
				return;
			}

			_logger?.LogWarning("Invalid request received {Path}{Query}", context.Request.Path, context.Request.QueryString);

			await next.Invoke(context);
		}
	}
}
