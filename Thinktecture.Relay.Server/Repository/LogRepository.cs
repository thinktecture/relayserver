using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Thinktecture.Relay.Server.Dto;
using Thinktecture.Relay.Server.Repository.DbModels;

namespace Thinktecture.Relay.Server.Repository
{
	public class LogRepository : ILogRepository
	{
		public void LogRequest(RequestLogEntry requestLogEntry)
		{
			using (var context = new RelayContext())
			{
				var link = new DbLink
				{
					Id = requestLogEntry.LinkId
				};

				context.Links.Attach(link);

				context.RequestLogEntries.Add(new DbRequestLogEntry
				{
					Id = Guid.NewGuid(),
					ContentBytesIn = requestLogEntry.ContentBytesIn,
					ContentBytesOut = requestLogEntry.ContentBytesOut,
					OnPremiseConnectorInDate = requestLogEntry.OnPremiseConnectorInDate,
					OnPremiseConnectorOutDate = requestLogEntry.OnPremiseConnectorOutDate,
					HttpStatusCode = requestLogEntry.HttpStatusCode,
					OnPremiseTargetInDate = requestLogEntry.OnPremiseTargetInDate,
					OnPremiseTargetKey = requestLogEntry.OnPremiseTargetKey,
					OnPremiseTargetOutDate = requestLogEntry.OnPremiseTargetOutDate,
					LocalUrl = requestLogEntry.LocalUrl ?? "/",
					OriginId = requestLogEntry.OriginId,
					Link = link,
					LinkId = requestLogEntry.LinkId
				});

				context.SaveChanges();
			}
		}

        public IEnumerable<RequestLogEntry> GetRecentLogEntriesForLink(Guid linkId,
            int amount)
        {
            using (var context = new RelayContext())
            {
                return
                    context.RequestLogEntries.Where(r => r.LinkId == linkId)
                        .OrderByDescending(e => e.OnPremiseConnectorOutDate)
                        .Take(amount)
                        .ToList()
                        .Select(d => new RequestLogEntry
                        {
                            OnPremiseConnectorInDate = d.OnPremiseConnectorInDate,
                            OnPremiseConnectorOutDate = d.OnPremiseConnectorOutDate,
                            ContentBytesIn = d.ContentBytesIn,
                            ContentBytesOut = d.ContentBytesOut,
                            HttpStatusCode = d.HttpStatusCode,
                            OnPremiseTargetInDate = d.OnPremiseTargetInDate,
                            OnPremiseTargetOutDate = d.OnPremiseTargetOutDate,
                            OnPremiseTargetKey = d.OnPremiseTargetKey,
                            LocalUrl = d.LocalUrl,
                            OriginId = d.OriginId,
                            LinkId = d.LinkId
                        });
            }
        }

        public IEnumerable<RequestLogEntry> GetRecentLogEntries(int amount)
        {
            using (var context = new RelayContext())
            {
                return context.RequestLogEntries
                    .OrderByDescending(e => e.OnPremiseConnectorOutDate)
                    .Take(amount)
                    .Select(d => new RequestLogEntry()
                    {
                        OnPremiseConnectorInDate = d.OnPremiseConnectorInDate,
                        OnPremiseConnectorOutDate = d.OnPremiseConnectorOutDate,
                        ContentBytesIn = d.ContentBytesIn,
                        ContentBytesOut = d.ContentBytesOut,
                        HttpStatusCode = d.HttpStatusCode,
                        OnPremiseTargetInDate = d.OnPremiseTargetInDate,
                        OnPremiseTargetOutDate = d.OnPremiseTargetOutDate,
                        OnPremiseTargetKey = d.OnPremiseTargetKey,
                        LocalUrl = d.LocalUrl,
                        OriginId = d.OriginId,
                        LinkId = d.LinkId
                    })
                    .ToList();
            }
        }

        public IEnumerable<ContentBytesChartDataItem> GetContentBytesChartDataItemsForLink(Guid id, TimeFrame timeFrame)
        {
            using (var context = new RelayContext())
            {
                var query = CreateBasicChartDataQuery(context, id, timeFrame);

                var result = AggregateContentBytesChartData(query, timeFrame);

                return result;
            }
        }

        internal IEnumerable<ContentBytesChartDataItem> AggregateContentBytesChartData(
            IQueryable<DbRequestLogEntry> query, TimeFrame timeFrame)
        {
            var groupedQuery = GetGrouping(query, timeFrame.Resolution);

            return groupedQuery
                .Select(group => new ContentBytesChartDataItem()
                {
                    In = group.Sum(entry => entry.ContentBytesIn),
                    Out = group.Sum(entry => entry.ContentBytesOut),
                    Key = DbFunctions.CreateDateTime(group.Key.Year, group.Key.Month ?? 1, group.Key.Day ?? 1, 0, 0, 0).Value
                })
                .OrderBy(entry => entry.Key)
                .ToList();
        }

        internal IQueryable<IGrouping<ContentBytesChartGroupKey, DbRequestLogEntry>> GetGrouping(
            IQueryable<DbRequestLogEntry> query,
            Resolution resolution)
        {
            if (resolution == Resolution.Daily)
            {
                return query.GroupBy(entry => new ContentBytesChartGroupKey()
                {
                    Year = entry.OnPremiseConnectorInDate.Year,
                    Month = entry.OnPremiseConnectorInDate.Month,
                    Day = entry.OnPremiseConnectorInDate.Day
                });
            }

            if (resolution == Resolution.Monthly)
            {
                return query.GroupBy(entry => new ContentBytesChartGroupKey()
                {
                    Year = entry.OnPremiseConnectorInDate.Year,
                    Month = entry.OnPremiseConnectorInDate.Month,
                    Day = null
                });
            }

            return query.GroupBy(entry => new ContentBytesChartGroupKey()
            {
                Year = entry.OnPremiseConnectorInDate.Year,
                Month = null,
                Day = null
            });
        }

        public IEnumerable<ContentBytesChartDataItem> GetContentBytesChartDataItems()
        {
            var timeFrame = new TimeFrame()
            {
                Start = DateTime.Now.AddDays(-7),
                End = DateTime.Now,
                Resolution = Resolution.Daily
            };

            return GetContentBytesChartDataItemsForLink(Guid.Empty, timeFrame);
        }


        private IQueryable<DbRequestLogEntry> CreateBasicChartDataQuery(RelayContext context, Guid id,
            TimeFrame timeFrame)
        {
            var startDate = new DateTime(timeFrame.Start.Year, timeFrame.Start.Month, timeFrame.Start.Day);
            var endDate = new DateTime(timeFrame.End.Year, timeFrame.End.Month, timeFrame.End.Day, 23, 59, 59, 999);

            var query = context.RequestLogEntries
                .Where(log => log.OnPremiseConnectorInDate >= startDate)
                .Where(log => log.OnPremiseConnectorOutDate <= endDate);

            if (id != Guid.Empty)
            {
                query = query.Where(log => log.LinkId == id);
            }

            query = query.OrderBy(log => log.OnPremiseConnectorInDate);
            return query;
        }
    }
}