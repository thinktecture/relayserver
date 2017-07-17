using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.Relay.Server.Communication
{
	internal class ConnectionInformation
	{
		public string UserName { get; }
		public string Role { get; }
		public string OnPremiseId { get; }

		public ConnectionInformation(string onPremiseId, string userName, string role)
		{
			UserName = userName;
			Role = role;
			OnPremiseId = onPremiseId;
		}
	}
}