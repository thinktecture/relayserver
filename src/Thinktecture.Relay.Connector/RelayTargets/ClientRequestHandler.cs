using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Connector.DependencyInjection;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.RelayTargets
{
	/// <inheritdoc />
	internal class ClientRequestHandler<TRequest, TResponse> : IClientRequestHandler<TRequest, TResponse>, IDisposable
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		private readonly Dictionary<string, RelayTargetRegistration<TRequest, TResponse>> _targets
			= new Dictionary<string, RelayTargetRegistration<TRequest, TResponse>>(StringComparer.InvariantCultureIgnoreCase);

		private readonly IServiceProvider _serviceProvider;
		private readonly HttpClient _httpClient;
		private readonly Uri _responseEndpoint;

		/// <summary>
		/// Initializes a new instance of <see cref="ClientRequestHandler{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="serviceProvider">An <see cref="IServiceProvider"/>.</param>
		/// <param name="targets">The registered <see cref="IRelayTarget{TRequest,TResponse}"/>s.</param>
		/// <param name="httpClientFactory">An <see cref="IHttpClientFactory"/>.</param>
		/// <param name="options">An <see cref="IOptions{TOptions}"/>.</param>
		public ClientRequestHandler(IServiceProvider serviceProvider, IEnumerable<RelayTargetRegistration<TRequest, TResponse>> targets,
			IHttpClientFactory httpClientFactory, IOptions<RelayConnectorOptions> options)
		{
			if (httpClientFactory == null)
			{
				throw new ArgumentNullException(nameof(httpClientFactory));
			}

			if (options == null)
			{
				throw new ArgumentNullException(nameof(options));
			}

			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_httpClient = httpClientFactory.CreateClient(Constants.RelayServerHttpClientName);
			_responseEndpoint = new Uri(options.Value.DiscoveryDocument.ResponseEndpoint);

			foreach (var target in targets)
			{
				_targets[target.Id] = target;
			}
		}

		/// <inheritdoc />
		public async Task<TResponse> HandleAsync(TRequest request, int? binarySizeThreshold, CancellationToken cancellationToken = default)
		{
			if (!TryGetTarget(request.Target, out var target) && !TryGetTarget(Constants.RelayTargetCatchAllId, out target))
			{
				return default;
			}

			try
			{
				var response = await target.HandleAsync(request, cancellationToken);

				if (response.BodySize > binarySizeThreshold)
				{
					// _httpClient.PostAsync()
					// TODO post to relay
				}
				else
				{
					response.BodyContent = await response.BodyContent.CopyToMemoryStreamAsync(cancellationToken);
				}

				// TODO optional post body first
				return response;
			}
			finally
			{
				(target as IDisposable)?.Dispose();
			}
		}

		private bool TryGetTarget(string id, out IRelayTarget<TRequest, TResponse> target)
		{
			if (_targets.TryGetValue(id, out var registration))
			{
				target = registration.Factory(_serviceProvider);
				return true;
			}

			target = null;
			return false;
		}

		public void Dispose() => _httpClient.Dispose();
	}
}
