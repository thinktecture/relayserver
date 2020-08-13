using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Connector.Docker
{
	public static class Startup
	{
		// This method gets called by the runtime. Use this method to add services to the container.
		public static void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
		{
			var configuration = hostBuilderContext.Configuration;

			services.AddRelayConnector(options =>
			{
				configuration.GetSection("RelayConnector").Bind(options);
			});

			services.AddHostedService<DummyLogger>();
		}
	}

	internal class DummyLogger : IHostedService, IDisposable
	{
		private readonly ILogger<DummyLogger> _logger;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		private Task _runningTask;

		public DummyLogger(ILogger<DummyLogger> logger, IHttpClientFactory httpClientFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_runningTask = RunAsync(_cancellationTokenSource.Token);

			// If the task is completed then return it,
			// this will bubble cancellation and failure to the caller
			if (_runningTask.IsCompleted)
			{
				return _runningTask;
			}

			return Task.CompletedTask;
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			if (_runningTask == null)
			{
				return;
			}

			try
			{
				// Try to stop the running task.
				_cancellationTokenSource.Cancel();
			}
			finally
			{
				// Wait for the task to stop
				await Task.WhenAny(_runningTask, Task.Delay(Timeout.Infinite, cancellationToken));
			}
		}

		public async Task RunAsync(CancellationToken cancellationToken)
		{
			int i = 0;

			while (!cancellationToken.IsCancellationRequested)
			{
				_logger.LogInformation("Internal loop running at {Time} and counting {i}", DateTime.UtcNow, i++);

				var client = _httpClientFactory.CreateClient(Constants.RelayServerHttpClientName);
				var response = await client.GetAsync("/.well-known/relayserver-configuration", cancellationToken);
				if (response.IsSuccessStatusCode)
				{
					var contentStream = await response.Content.ReadAsStreamAsync();
					var document = await JsonSerializer.DeserializeAsync<DiscoveryDocument>(contentStream, new JsonSerializerOptions()
					{
						PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
					}, cancellationToken);
					_logger.LogInformation("Read discovery document: {@DiscoveryDocument}", document);
				}
				else
				{
					_logger.LogInformation("Fetching document failed.");
				}

				await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
			}
		}

		public void Dispose()
		{
			_cancellationTokenSource.Cancel();
		}
	}
}
