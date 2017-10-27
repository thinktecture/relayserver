using System;
using System.Web.Http.Tracing;
using NLog;

namespace Thinktecture.Relay.Server.Logging
{
	public class TraceLevelConverter : ITraceLevelConverter
	{
		public LogLevel Convert(TraceLevel level)
		{
			switch (level)
			{
				case TraceLevel.Off:
					return LogLevel.Off;
				case TraceLevel.Debug:
					return LogLevel.Debug;
				case TraceLevel.Info:
					return LogLevel.Info;
				case TraceLevel.Warn:
					return LogLevel.Warn;
				case TraceLevel.Error:
					return LogLevel.Error;
				case TraceLevel.Fatal:
					return LogLevel.Fatal;
				default:
					throw new ArgumentOutOfRangeException(nameof(level), level, null);
			}
		}
	}
}
