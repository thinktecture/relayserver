namespace Thinktecture.Relay.Server.Communication
{
	internal class ConnectionInformation
	{
		public string UserName { get; }
		public string Role { get; }
		public string LinkId { get; }

		public ConnectionInformation(string onPremiseId, string userName, string role)
		{
			UserName = userName;
			Role = role;
			LinkId = onPremiseId;
		}
	}
}
