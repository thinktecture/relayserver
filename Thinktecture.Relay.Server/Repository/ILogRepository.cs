using System;
using System.Collections.Generic;
using Thinktecture.Relay.Server.Dto;

namespace Thinktecture.Relay.Server.Repository
{
	public interface ILogRepository
	{
		void LogRequest(RequestLogEntry requestLogEntry);
		IEnumerable<RequestLogEntry> GetRecentLogEntriesForLink(Guid linkId, int amount);
        IEnumerable<ContentBytesChartDataItem> GetContentBytesChartDataItemsForLink(Guid id, TimeFrame timeFrame);
	    IEnumerable<ContentBytesChartDataItem> GetContentBytesChartDataItems();
	    IEnumerable<RequestLogEntry> GetRecentLogEntries(int amount);
	}
}