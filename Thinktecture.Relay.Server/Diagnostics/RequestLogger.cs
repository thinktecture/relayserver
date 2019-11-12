using System;
using System.Net;
using Thinktecture.Relay.Server.Dto;
using Thinktecture.Relay.Server.Helper;
using Thinktecture.Relay.Server.OnPremise;
using Thinktecture.Relay.Server.Repository;

namespace Thinktecture.Relay.Server.Diagnostics
{
	public class RequestLogger : IRequestLogger
	{
		private readonly ILogRepository _logRepository;
		private readonly IPathSplitter _pathSplitter;

		public RequestLogger(ILogRepository logRepository, IPathSplitter pathSplitter)
		{
			_logRepository = logRepository;
			_pathSplitter = pathSplitter;
		}

		public void LogRequest(IOnPremiseConnectorRequest request, IOnPremiseConnectorResponse response, Guid linkId, Guid originId, string relayPath, HttpStatusCode? statusCode)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			var pathInformation = _pathSplitter.Split(relayPath);

			_logRepository.LogRequest(new RequestLogEntry
			{
				LocalUrl = pathInformation.LocalUrl,
				HttpStatusCode = statusCode ?? HttpStatusCode.InternalServerError,
				ContentBytesIn = request.ContentLength,
				ContentBytesOut = response?.ContentLength ?? 0,
				OnPremiseConnectorInDate = request.RequestStarted,
				OnPremiseConnectorOutDate = request.RequestFinished,
				OnPremiseTargetInDate = response?.RequestStarted,
				OnPremiseTargetOutDate = response?.RequestFinished,
				OriginId = originId,
				OnPremiseTargetKey = pathInformation.OnPremiseTargetKey,
				LinkId = linkId,
				RequestId = request.RequestId,
			});
		}
	}
}
