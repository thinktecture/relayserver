using System.Collections;
using System.Collections.Generic;

namespace Thinktecture.Relay.Server.Dto
{
	public class Dashboard
	{
		public IEnumerable<RequestLogEntry> Logs { get; set; }
		public IEnumerable<ContentBytesChartDataItem> ContentBytesChartDataItems { get; set; }
	}
}
