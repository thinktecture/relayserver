using System;
using System.Collections.Concurrent;
using System.Reactive.Subjects;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication.InProcess
{
	public class RequestSubjectContext : IDisposable
	{
		public Subject<IOnPremiseConnectorRequest> Subject { get; }
		public int ConnectionCount => _connectionIds.Count;

		private readonly ConcurrentDictionary<string, string> _connectionIds;

		public RequestSubjectContext()
		{
			Subject = new Subject<IOnPremiseConnectorRequest>();
			_connectionIds = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		}

		public void AddConnection(string connectionId)
		{
			_connectionIds.TryAdd(connectionId, connectionId);
		}

		public void RemoveConnection(string connectionId)
		{
			_connectionIds.TryRemove(connectionId, out var id);
		}

		public void Dispose()
		{
			Subject.Dispose();
		}
	}
}
