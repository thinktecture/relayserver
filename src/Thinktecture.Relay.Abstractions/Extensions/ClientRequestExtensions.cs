using System.Collections.Generic;
using System.Net;
using Thinktecture.Relay.Acknowledgement;

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
		/// <param name="failureStatusCode">A <see cref="HttpStatusCode"/> which marks the target response as failed.</param>
		/// <returns>The <see cref="TargetResponse"/>.</returns>
		public static TargetResponse CreateResponse(this IClientRequest request, HttpStatusCode? failureStatusCode = null)
			=> request.CreateResponse<TargetResponse>(failureStatusCode);

		/// <summary>
		/// Creates a pristine <see cref="ITargetResponse"/> prefilled with <see cref="IClientRequest.RequestId"/> and <see cref="IClientRequest.RequestOriginId"/>.
		/// </summary>
		/// <param name="request">An <see cref="IClientRequest"/>.</param>
		/// <param name="failureStatusCode">A <see cref="HttpStatusCode"/> which marks the target response as failed.</param>
		/// <typeparam name="T">The type of response.</typeparam>
		/// <returns>The <see cref="ITargetResponse"/>.</returns>
		public static T CreateResponse<T>(this IClientRequest request, HttpStatusCode? failureStatusCode = null)
			where T : ITargetResponse, new()
		{
			var response = new T()
			{
				RequestId = request.RequestId,
				RequestOriginId = request.RequestOriginId,
				HttpHeaders = new Dictionary<string, string[]>()
			};

			if (failureStatusCode == null) return response;

			response.BodySize = 0;
			response.HttpStatusCode = failureStatusCode.Value;
			response.RequestFailed = true;

			return response;
		}

		/// <summary>
		/// Creates an <see cref="IAcknowledgeRequest"/> for the request.
		/// </summary>
		/// <param name="request">The client request.</param>
		/// <param name="removeRequestBodyContent">Indicates if the request body should be removed.</param>
		/// <typeparam name="T">The type of acknowledge.</typeparam>
		/// <returns>A instance of <see cref="IAcknowledgeRequest"/>.</returns>
		public static T CreateAcknowledge<T>(this IClientRequest request, bool removeRequestBodyContent)
			where T : IAcknowledgeRequest, new() => new T()
		{
			OriginId = request.RequestOriginId,
			RequestId = request.RequestId,
			RemoveRequestBodyContent = removeRequestBodyContent
		};
	}
}
