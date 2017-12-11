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

			_logger?.Write(_traceLevelConverter.Convert(level), record.Exception, null,
				"Category: {Category}, Operator: {Operator}, Kind: {Kind}, Operation: {Operation}, Properties: {Properties}, Message: {Message}, Exception: {Exception}",
				category, record.Operator, record.Kind, record.Operation, record.Properties, record.Message ?? "-", record.Exception?.ToString() ?? "-");
		}
	}
}
