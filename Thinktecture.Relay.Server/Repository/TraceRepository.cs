using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Thinktecture.Relay.Server.Dto;
using Thinktecture.Relay.Server.Repository.DbModels;

namespace Thinktecture.Relay.Server.Repository
{
	public class TraceRepository : ITraceRepository
	{
		public void Create(TraceConfiguration traceConfiguration)
		{
			using (var context = new RelayContext())
			{
			    var link = new DbLink
			    {
			        Id = traceConfiguration.LinkId
			    };

			    context.Links.Attach(link);

				context.TraceConfigurations.Add(new DbTraceConfiguration
				{
					CreationDate = DateTime.UtcNow,
					EndDate = traceConfiguration.EndDate,
					Id = Guid.NewGuid(),
                    Link = link,
					LinkId = traceConfiguration.LinkId,
					StartDate = traceConfiguration.StartDate
				});

			    context.SaveChanges();
			}
		}

		public bool Disable(Guid id)
		{
			using (var context = new RelayContext())
			{
				var dbTraceConfiguration = context.TraceConfigurations.Include(t => t.Link).SingleOrDefault(t => t.Id == id);

				if (dbTraceConfiguration == null)
				{
					return false;
				}

				if (dbTraceConfiguration.EndDate < DateTime.UtcNow)
				{
					return true;
				}

				dbTraceConfiguration.EndDate = DateTime.UtcNow;

                context.Entry(dbTraceConfiguration).State = EntityState.Modified;

				return context.SaveChanges() == 1;
			}
		}

		public Guid? GetCurrentTraceConfigurationId(Guid linkId)
		{
			using (var context = new RelayContext())
			{
				var traceConfiguration = context.TraceConfigurations.SingleOrDefault(t => t.StartDate < DateTime.UtcNow && t.EndDate > DateTime.UtcNow &&
					t.LinkId == linkId);

			    return traceConfiguration?.Id;
			}
		}

		public IEnumerable<TraceConfiguration> GetTraceConfigurations(Guid linkId)
		{
			using (var context = new RelayContext())
			{
			    return context.TraceConfigurations.Where(t => t.LinkId == linkId)
                    .OrderByDescending(t => t.CreationDate)
			        .Select(d => new TraceConfiguration
			        {
			            CreationDate = d.CreationDate,
			            EndDate = d.EndDate,
			            Id = d.Id,
			            LinkId = d.LinkId,
			            StartDate = d.StartDate
			        })
			        .ToList();
			}
		}

	    public TraceConfiguration GetRunningTranceConfiguration(Guid linkId)
	    {
	        using (var context = new RelayContext())
	        {
	            var id = GetCurrentTraceConfigurationId(linkId);

	            if (id == null)
	            {
	                return null;
	            }

	            return context.TraceConfigurations
	                .Where(t => t.Id == id.Value)
	                .Select(d => new TraceConfiguration
	                {
	                    CreationDate = d.CreationDate,
	                    EndDate = d.EndDate,
	                    Id = d.Id,
	                    LinkId = d.LinkId,
	                    StartDate = d.StartDate
	                }).SingleOrDefault();
	        }
	    }

	    public TraceConfiguration GetTraceConfiguration(Guid traceConfigurationId)
	    {
	        using (var context = new RelayContext())
	        {
	            return context.TraceConfigurations
	                .Where(t => t.Id == traceConfigurationId)
	                .Select(t => new TraceConfiguration
	                {
	                    CreationDate = t.CreationDate,
	                    EndDate = t.EndDate,
	                    Id = t.Id,
	                    LinkId = t.LinkId,
	                    StartDate = t.StartDate
	                }).SingleOrDefault();
	        }
	    }
	}
}