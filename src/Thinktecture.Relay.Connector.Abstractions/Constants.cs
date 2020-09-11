namespace Thinktecture.Relay.Connector
{
	/// <summary>
	/// Constants for the connector.
	/// </summary>
	public static class Constants
	{
		/// <summary>
		/// The identity scopes.
		/// </summary>
		public const string RelayServerScopes = "relaying";

		/// <summary>
		/// The id for the catch-all target.
		/// </summary>
		public const string RelayTargetCatchAllId = "** CATCH-ALL **";

		/// <summary>
		/// The name of the http client used for communicating with the server.
		/// </summary>
		public const string RelayServerHttpClientName = "relayserver";
	}
}