using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Server;
using Thinktecture.Relay.Server.Diagnostics;
using Thinktecture.Relay.Server.Persistence;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace ExampleRelayServer.Samples;

public class SampleMetadataRequestLogger<TRequest, TResponse> : RelayRequestLogger<TRequest, TResponse>
	where TRequest : IClientRequest
	where TResponse : class, ITargetResponse
{
	public SampleMetadataRequestLogger(ILogger<SampleMetadataRequestLogger<TRequest, TResponse>> logger,
		IRequestService requestService,
		IOptions<RelayServerOptions> relayServerOptions)
		: base(logger, requestService, relayServerOptions)
	{
	}

	public override async Task LogSuccessAsync(IRelayContext<TRequest, TResponse> relayContext)
	{
		await base.LogSuccessAsync(relayContext);
		await LogMetadataAsync(relayContext);
	}

	public override async Task LogAbortAsync(IRelayContext<TRequest, TResponse> relayContext)
	{
		await base.LogAbortAsync(relayContext);
		await LogMetadataAsync(relayContext);
	}

	public override async Task LogFailAsync(IRelayContext<TRequest, TResponse> relayContext)
	{
		await base.LogFailAsync(relayContext);
		await LogMetadataAsync(relayContext);
	}

	public override async Task LogExpiredAsync(IRelayContext<TRequest, TResponse> relayContext)
	{
		await base.LogExpiredAsync(relayContext);
		await LogMetadataAsync(relayContext);
	}

	public override async Task LogErrorAsync(IRelayContext<TRequest, TResponse> relayContext)
	{
		await base.LogErrorAsync(relayContext);
		await LogMetadataAsync(relayContext);
	}

	protected async Task LogMetadataAsync(IRelayContext<TRequest, TResponse> relayContext)
	{
		try
		{
			if (!relayContext.ClientRequest.HttpHeaders.TryGetValue("tt-relay-metadata", out var content))
				return;

			var header = content.FirstOrDefault();
			if (String.IsNullOrEmpty(header)) return;

			var json = Encoding.UTF8.GetString(Convert.FromBase64String(header));
			var metadata = JsonSerializer.Deserialize<Metadata>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

			Logger.LogInformation("Received request metadata: {@Metadata}", metadata);
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "Error while logging metadata.");
		}
	}

	private class File
	{
		public string FileName { get; set; }
		public long FileSize { get; set; }
	}

	private class Metadata
	{
		public Guid TraceId { get; set; }
		public string Client { get; set; }
		public DateTimeOffset Timestamp { get; set; }

		public string SourceTenant { get; set; }
		public string SourceApplication { get; set; }
		public string SourceFolder { get; set; }
		public string SourceMessageType { get; set; }

		public File[] Files { get; set; }
	}
}
