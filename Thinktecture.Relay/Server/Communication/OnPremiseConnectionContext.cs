using System;
using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication
{
	/// <inheritdoc/>
	public class OnPremiseConnectionContext : IOnPremiseConnectionContext
	{
		/// <inheritdoc/>
		public string ConnectionId { get; set; }

		/// <inheritdoc/>
		public Guid LinkId { get; set; }

		/// <inheritdoc/>
		public bool IsActive { get; set; } = true;

		/// <inheritdoc/>
		public DateTime LastLocalActivity { get; set; } = DateTime.UtcNow;

		/// <inheritdoc/>
		public Func<IOnPremiseConnectorRequest, CancellationToken, Task> RequestAction { get; set; }

		/// <inheritdoc/>
		public string IpAddress { get; set; }

		/// <inheritdoc/>
		public string UserName { get; set; }

		/// <inheritdoc/>
		public string Role { get; set; }

		/// <inheritdoc/>
		public int ConnectorVersion { get; set; }

		/// <inheritdoc/>
		public string ConnectorAssemblyVersion { get; set; }

		/// <inheritdoc/>
		public virtual bool SupportsAck => ConnectorVersion >= 1;

		/// <inheritdoc/>
		public virtual bool SupportsHeartbeat => ConnectorVersion >= 2;

		/// <inheritdoc/>
		public virtual bool SupportsConfiguration => ConnectorVersion >= 3;

		/// <inheritdoc/>
		public DateTime NextHeartbeat { get; set; }
	}
}
