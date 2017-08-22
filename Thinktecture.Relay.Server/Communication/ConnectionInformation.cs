using System;

namespace Thinktecture.Relay.Server.Communication
{
	internal class ConnectionInformation
	{
		public string UserName { get; }
		public string Role { get; }
		public Guid LinkId { get; }

		public ConnectionInformation(Guid linkId, string userName, string role)
		{
			UserName = userName;
			Role = role;
			LinkId = linkId;
		}
	}
}
