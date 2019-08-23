namespace Thinktecture.Relay.OnPremiseConnector.Interceptor
{
	/// <summary>
	/// Represents an interceptor that can modify requests.
	/// </summary>
	public interface IOnPremiseRequestInterceptor
	{
		/// <summary>
		/// Will be called when a request was intercepted.
		/// </summary>
		/// <param name="request">The <see cref="IInterceptedRequest"/> that was intercepted.</param>
		void HandleRequest(IInterceptedRequest request);
	}
}
