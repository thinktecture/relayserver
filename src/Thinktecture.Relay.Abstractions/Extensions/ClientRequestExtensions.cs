using System.Collections.Generic;
using System.Net;

// ReSharper disable once CheckNamespace; (extension methods on IClientRequest namespace)
namespace Thinktecture.Relay.Transport
{
	/// <summary>
	/// Extension methods for <see cref="IClientRequest"/>.
	/// </summary>
	public static class ClientRequestExtensions
	{
		/// <summary>
		/// Checks if the body content is currently outsourced.
		/// </summary>
		/// <param name="request">An <see cref="IClientRequest"/>.</param>
		/// <returns>true if the body content is outsourced; otherwise, false.</returns>
		public static bool IsBodyContentOutsourced(this IClientRequest request) => request.BodySize > 0 && request.BodyContent == null;

		/// <summary>
		/// Creates a pristine <see cref="TargetResponse"/> prefilled with <see cref="IClientRequest.RequestId"/> and <see cref="IClientRequest.RequestOriginId"/>.
		/// </summary>
		/// <param name="request">An <see cref="IClientRequest"/>.</param>
		/// <param name="failureStatusCode">A <see cref="HttpStatusCode"/> which marks the response as failed.</param>
		/// <returns>The <see cref="TargetResponse"/>.</returns>
		public static TargetResponse CreateResponse(this IClientRequest request, HttpStatusCode? failureStatusCode = null)
			=> request.CreateResponse<TargetResponse>(failureStatusCode);

		/// <summary>
		/// Creates a pristine <see cref="ITargetResponse"/> prefilled with <see cref="IClientRequest.RequestId"/> and <see cref="IClientRequest.RequestOriginId"/>.
		/// </summary>
		/// <param name="request">An <see cref="IClientRequest"/>.</param>
		/// <param name="failureStatusCode">A <see cref="HttpStatusCode"/> which marks the response as failed.</param>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <returns>The <see cref="ITargetResponse"/>.</returns>
		public static TResponse CreateResponse<TResponse>(this IClientRequest request, HttpStatusCode? failureStatusCode = null)
			where TResponse : ITargetResponse, new()
		{
			var response = new TResponse()
			{
				RequestId = request.RequestId,
				RequestOriginId = request.RequestOriginId,
				HttpHeaders = new Dictionary<string, string[]>()
			};

			if (failureStatusCode != null)
			{
				response.BodySize = 0;
				response.HttpStatusCode = failureStatusCode.Value;
				response.RequestFailed = true;
			}

			return response;
		}
	}
}
