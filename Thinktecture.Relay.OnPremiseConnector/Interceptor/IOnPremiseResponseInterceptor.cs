namespace Thinktecture.Relay.OnPremiseConnector.Interceptor
{
	/// <summary>
	/// Represents an interceptor that can modify responses.
	/// </summary>
	public interface IOnPremiseResponseInterceptor
	{
		/// <summary>
		/// Will be called when a response was intercepted.
		/// </summary>
		/// <param name="request">The <see cref="IInterceptedRequest"/> that lead to the response.</param>
		/// <param name="response">The <see cref="IInterceptedResponse"/> that was intercepted.</param>
		void HandleResponse(IInterceptedRequest request, IInterceptedResponse response);
	}
}
