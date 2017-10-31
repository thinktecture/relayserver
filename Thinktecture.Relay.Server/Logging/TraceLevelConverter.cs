using System;
using System.Web.Http.Tracing;
using Serilog.Events;

namespace Thinktecture.Relay.Server.Logging
{
	public class TraceLevelConverter : ITraceLevelConverter
	{
		public LogEventLevel Convert(TraceLevel level)
		{
			switch (level)
			{
				case TraceLevel.Off:
					return LogEventLevel.Verbose;
				case TraceLevel.Debug:
					return LogEventLevel.Debug;
				case TraceLevel.Info:
					return LogEventLevel.Information;
				case TraceLevel.Warn:
					return LogEventLevel.Warning;
				case TraceLevel.Error:
					return LogEventLevel.Error;
				case TraceLevel.Fatal:
					return LogEventLevel.Fatal;
				default:
					throw new ArgumentOutOfRangeException(nameof(level), level, null);
			}
		}
	}
}
