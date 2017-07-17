using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Tracing;
using NLog;

namespace Thinktecture.Relay.Server.Logging
{
	public interface ITraceLevelConverter
	{
		LogLevel Convert(TraceLevel level);
	}
}