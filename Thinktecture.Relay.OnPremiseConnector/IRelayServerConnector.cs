using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Thinktecture.Relay.OnPremiseConnector
{
	/// <summary>
	/// Interface that represents the class connecting to the relay server.
	/// </summary>
	public interface IRelayServerConnector
	{
		/// <summary>
		/// Gets or sets the header that is set on relayed requests.
		/// </summary>
		string RelayedRequestHeader { get; set; }

		/// <summary>
		/// Raised, when a connection within the connector connects to the RelayServer.
		/// </summary>
		event EventHandler Connected;

		/// <summary>
		/// Raised, when a connection within the connector disconnects from the RelayServer.
		/// </summary>
		event EventHandler Disconnected;

		/// <summary>
		/// Registers a on-premise web target.
		/// </summary>
		/// <param name="key">A <see cref="String"/> defining the key for the target.</param>
		/// <param name="uri">An <see cref="Uri"/> containing the on-premise target's base url.</param>
		void RegisterOnPremiseTarget(string key, Uri uri);

		/// <summary>
		/// Registers a on-premise in-proc target.
		/// </summary>
		/// <param name="key">A <see cref="String"/> defining the key for the target.</param>
		/// <param name="handlerType">A <see cref="Type"/> implementing <see cref="IOnPremiseInProcHandler"/>.</param>
		void RegisterOnPremiseTarget(string key, Type handlerType);

		/// <summary>
		/// Registers a on-premise in-proc target.
		/// </summary>
		/// <param name="key">A <see cref="String"/> defining the key for the target.</param>
		/// <param name="handlerFactory">Creates handler.</param>
		void RegisterOnPremiseTarget(string key, Func<IOnPremiseInProcHandler> handlerFactory);

		/// <summary>
		/// Registers a on-premise in-proc target.
		/// </summary>
		/// <param name="key">A <see cref="String"/> defining the key for the target.</param>
		/// <typeparam name="T">The type of the handler.</typeparam>
		void RegisterOnPremiseTarget<T>(string key) where T : IOnPremiseInProcHandler, new();

		/// <summary>
		/// Removes a on-premise target.
		/// </summary>
		/// <param name="key">A <see cref="String"/> defining the key for the target.</param>
		void RemoveOnPremiseTarget(string key);

		/// <summary>
		/// Returns the list of configured target keys
		/// </summary>
		List<string> GetOnPremiseTargetKeys();

		/// <summary>
		/// Connects to the RelayServer.
		/// </summary>
		Task ConnectAsync();

		/// <summary>
		/// Disconnects from the RelayServer.
		/// </summary>
		void Disconnect();

		/// <summary>
		/// Makes a GET request to RelayServer using the current authentication token.
		/// </summary>
		/// <param name="linkName">The name of the relay link.</param>
		/// <param name="relativeUrl">Url relative to the RelayServer url.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Http response</returns>
		Task<HttpResponseMessage> GetViaRelay(string linkName, string relativeUrl, CancellationToken cancellationToken);

		/// <summary>
		/// Makes a GET request to RelayServer using the current authentication token.
		/// </summary>
		/// <param name="linkName">The name of the relay link.</param>
		/// <param name="relativeUrl">Url relative to the RelayServer url.</param>
		/// <param name="setHeaders">Callback for setting headers.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Http response</returns>
		Task<HttpResponseMessage> GetViaRelay(string linkName, string relativeUrl, Action<HttpRequestHeaders> setHeaders, CancellationToken cancellationToken);

		/// <summary>
		/// Makes a POST request to RelayServer using the current authentication token.
		/// </summary>
		/// <param name="linkName">The name of the relay link.</param>
		/// <param name="relativeUrl">Url relative to the RelayServer url.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Http response</returns>
		Task<HttpResponseMessage> PostViaRelay(string linkName, string relativeUrl, CancellationToken cancellationToken);

		/// <summary>
		/// Makes a POST request to RelayServer using the current authentication token.
		/// </summary>
		/// <param name="linkName">The name of the relay link.</param>
		/// <param name="relativeUrl">Url relative to the RelayServer url.</param>
		/// <param name="setHeaders">Callback for setting headers.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Http response</returns>
		Task<HttpResponseMessage> PostViaRelay(string linkName, string relativeUrl, Action<HttpRequestHeaders> setHeaders, CancellationToken cancellationToken);

		/// <summary>
		/// Makes a POST request to RelayServer using the current authentication token.
		/// </summary>
		/// <param name="linkName">The name of the relay link.</param>
		/// <param name="relativeUrl">Url relative to the RelayServer url.</param>
		/// <param name="content">The <see cref="HttpContent"/> to post through the RelayServer.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Http response</returns>
		Task<HttpResponseMessage> PostViaRelay(string linkName, string relativeUrl, HttpContent content, CancellationToken cancellationToken);

		/// <summary>
		/// Makes a POST request to RelayServer using the current authentication token.
		/// </summary>
		/// <param name="linkName">The name of the relay link.</param>
		/// <param name="relativeUrl">Url relative to the RelayServer url.</param>
		/// <param name="setHeaders">Callback for setting headers.</param>
		/// <param name="content">The <see cref="HttpContent"/> to post through the RelayServer.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Http response</returns>
		Task<HttpResponseMessage> PostViaRelay(string linkName, string relativeUrl, Action<HttpRequestHeaders> setHeaders, HttpContent content, CancellationToken cancellationToken);
	}
}
