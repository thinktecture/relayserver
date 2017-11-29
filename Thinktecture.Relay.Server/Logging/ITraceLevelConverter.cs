using System.Web.Http.Tracing;
using Serilog.Events;

namespace Thinktecture.Relay.Server.Logging
{
	public interface ITraceLevelConverter
	{
		LogEventLevel Convert(TraceLevel level);
	}
}
