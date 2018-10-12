using System.Net.Http;

namespace Thinktecture.Relay.OnPremiseConnector.Net.Http
{
	/// <summary>
	/// A factory abstraction for a component that can create <see cref="HttpClient"/> instances with custom
	/// configuration for a given logical name.
	/// </summary>
	public interface IHttpClientFactory
	{
		/// <summary>
		/// Creates and configures an <see cref="HttpClient"/> instance using the configuration that corresponds
		/// to the logical name specified by <paramref name="name"/>.
		/// </summary>
		/// <param name="name">The logical name of the client to create.</param>
		/// <returns>A new <see cref="HttpClient"/> instance.</returns>
		/// <remarks>
		/// <para>
		/// Each call to <see cref="CreateClient(string)"/> is guaranteed to return a new <see cref="HttpClient"/>
		/// instance. Callers may cache the returned <see cref="HttpClient"/> instance indefinitely or surround
		/// its use in a <langword>using</langword> block to dispose it when desired.
		/// </para>
		/// <para>
		/// The default <see cref="IHttpClientFactory"/> implementation may cache the underlying
		/// <see cref="HttpMessageHandler"/> instances to improve performance.
		/// </para>
		/// <para>
		/// Callers are also free to mutate the returned <see cref="HttpClient"/> instance's public properties
		/// as desired.
		/// </para>
		/// </remarks>
		HttpClient CreateClient(string name);
	}
}
