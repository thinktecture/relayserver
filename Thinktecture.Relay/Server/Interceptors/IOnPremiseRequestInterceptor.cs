using System.Net.Http;

namespace Thinktecture.Relay.Server.Interceptors
{
	/// <summary>
	/// Interface for interceptors that are capable of modifying the incoming request before it gets relayed to its destination.
	/// </summary>
	public interface IOnPremiseRequestInterceptor
	{
		/// <summary>
		/// This method can modify the request and prevent further processing by returning an <see cref="HttpResponseMessage"/>.
		/// </summary>
		/// <param name="request">The request that can be modified.</param>
		/// <returns>If the returned <see cref="HttpResponseMessage"/> is not null then it will immidiately be send out to the client without any further processing.</returns>
		HttpResponseMessage OnRequestReceived(IInterceptedRequest request);
	}
}
