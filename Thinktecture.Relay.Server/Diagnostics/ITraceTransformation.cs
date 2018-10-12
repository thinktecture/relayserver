using System.Net.Http;

namespace Thinktecture.Relay.Server.Diagnostics
{
	public interface ITraceTransformation
	{
		HttpResponseMessage CreateFromTraceFile(TraceFile traceFile);
	}
}
