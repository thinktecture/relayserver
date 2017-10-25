using System.Net.Http;

namespace Thinktecture.Relay.Server.Interceptors
{
	/// <summary>
	/// Interface for interceptors that are capable of modifying the outgoing response after it was received from the remote destination.
	/// </summary>
	public interface IOnPremiseResponseInterceptor
	{
		/// <summary>
		/// This method is called if no response could be fetched for given <paramref name="request"/>.
		/// </summary>
		/// <param name="request">The original request.</param>
		/// <returns>If the returned <see cref="HttpResponseMessage"/> is not null then it will immidiately be send out to the client without any further processing.</returns>
		HttpResponseMessage OnResponseReceived(IReadOnlyInterceptedRequest request);

		/// <summary>
		/// This method can modify the response and prevent further processing by returning an <see cref="HttpResponseMessage"/>.
		/// </summary>
		/// <param name="request">The original request that the <paramref name="response"/> is meant for.</param>
		/// <param name="response">The response from the remote location that is to be modified, or null if no response could be retrieved.</param>
		/// <returns>If the returned <see cref="HttpResponseMessage"/> is not null then it will immidiately be send out to the client without any further processing.</returns>
		HttpResponseMessage OnResponseReceived(IReadOnlyInterceptedRequest request, IInterceptedResponse response);
	}
}
