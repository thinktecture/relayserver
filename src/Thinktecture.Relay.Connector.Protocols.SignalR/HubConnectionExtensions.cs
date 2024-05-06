using System;
using Microsoft.AspNetCore.SignalR.Client;

namespace Thinktecture.Relay.Connector.Protocols.SignalR;

internal static class HubConnectionExtensions
{
	public static void SetKeepAliveInterval(this HubConnection connection, TimeSpan? keepAliveInterval)
	{
		if (keepAliveInterval is null) return;

		connection.KeepAliveInterval = keepAliveInterval.Value;
		connection.ServerTimeout =
			TimeSpan.FromSeconds(keepAliveInterval.Value.TotalSeconds * 2); // should always be twice of keep-alive
	}
}
