using System;
using System.Collections.Generic;

namespace Thinktecture.Relay.Server.Dto
{
	public class LinkDetails : Link
	{
		public string UserName { get; set; }
		public string Password { get; set; }
		public int MaximumLinks { get; set; }
		public DateTime CreationDate { get; set; }

		public bool IsConnected => Connections.Count > 0;

		private List<string> _connections;

		public List<string> Connections
		{
			get => _connections ?? (_connections = new List<string>());
			set => _connections = value;
		}
	}
}
