using System;
using System.Net.Http;
using System.Web.Http.Tracing;
using Serilog;

namespace Thinktecture.Relay.Server.Logging
{
	public class TraceWriter : ITraceWriter
	{
		private readonly ILogger _logger;
		private readonly ITraceLevelConverter _traceLevelConverter;

		public TraceWriter(ILogger logger, ITraceLevelConverter traceLevelConverter)
		{
			_logger = logger;
			_traceLevelConverter = traceLevelConverter ?? throw new ArgumentNullException(nameof(traceLevelConverter));
		}

		public void Trace(HttpRequestMessage request, string category, TraceLevel level, Action<TraceRecord> traceAction)
		{
			var record = new TraceRecord(request, category, level);
			traceAction(record);

			_logger?.Write(_traceLevelConverter.Convert(level), null, null,
				"Category: {0}, Operator: {1}, Kind: {2}, Operation: {3}, Properties: {4}, Message: {5}, Exception: {6}",
				category, record.Operator, record.Kind, record.Operation, record.Properties, record.Message ?? "-", record.Exception?.ToString() ?? "-");
		}
	}
}
