using System.Net.Http;
using Thinktecture.Relay.Connector.Targets;

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
		/// The id for the catch-all <see cref="IRelayTarget{TRequest,TResponse}"/>.
		/// </summary>
		public const string RelayTargetCatchAllId = "** CATCH-ALL **";

		/// <summary>
		/// Constants for HTTP headers.
		/// </summary>
		public static class HeaderNames
		{
			/// <summary>
			/// The url to use for acknowledging a request by issuing as POST with an empty body to it.
			/// </summary>
			/// <remarks>This will only be present when manual acknowledgement is needed.</remarks>
			public const string AcknowledgeUrl = "X-RelayServer-AcknowledgeUrl";

			/// <summary>
			/// The unique id of the request.
			/// </summary>
			public const string RequestId = "X-RelayServer-RequestId";

			/// <summary>
			/// The unique id of the origin receiving the request.
			/// </summary>
			public const string OriginId = "X-RelayServer-OriginId";

			/// <summary>
			/// The machine name of the connector handling the request.
			/// </summary>
			public const string ConnectorMachineName = "X-RelayServer-Connector-MachineName";

			/// <summary>
			/// The version of the connector handling the request.
			/// </summary>
			public const string ConnectorVersion = "X-RelayServer-Connector-Version";
		}

		/// <summary>
		/// The name of the configuration key in a target definition for the id.
		/// </summary>
		public const string RelayConnectorOptionsTargetId = "Id";

		/// <summary>
		/// The name of the configuration key in a target definition for the type.
		/// </summary>
		public const string RelayConnectorOptionsTargetType = "Type";

		/// <summary>
		/// The name of the configuration key in a target definition for the timeout.
		/// </summary>
		public const string RelayConnectorOptionsTargetTimeout = "Timeout";

		/// <summary>
		/// Constants for named <see cref="HttpClient"/>.
		/// </summary>
		public static class HttpClientNames
		{
			/// <summary>
			/// The name of the <see cref="HttpClient"/> used for communicating with the server.
			/// </summary>
			public static readonly string RelayServer = $"{typeof(Constants).Namespace}.{nameof(RelayServer)}";

			/// <summary>
			/// The name of the default <see cref="HttpClient"/>.
			/// </summary>
			public static readonly string RelayWebTargetDefault = $"{typeof(Constants).Namespace}.{nameof(RelayWebTargetDefault)}";

			/// <summary>
			/// The name of the <see cref="HttpClient"/> following redirects.
			/// </summary>
			public static readonly string RelayWebTargetFollowRedirect =
				$"{typeof(Constants).Namespace}.{nameof(RelayWebTargetFollowRedirect)}";
		}
	}
}
