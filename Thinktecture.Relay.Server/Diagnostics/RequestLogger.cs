using System;
using System.Net;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
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

		public void LogRequest(IOnPremiseConnectorRequest onPremiseConnectorRequest, IOnPremiseTargetResponse onPremiseTargetResponse, HttpStatusCode responseStatusCode, Guid linkId, string originId, string relayPath)
		{
			if (onPremiseConnectorRequest == null)
			{
				throw new ArgumentNullException("onPremiseConnectorRequest", "A client request must be set");
			}

			var onPremiseTargetInformation = GetOnPremiseTargetInformation(onPremiseTargetResponse);
			var pathInformation = _pathSplitter.Split(relayPath);

			_logRepository.LogRequest(new RequestLogEntry
			{
				LocalUrl = pathInformation.LocalUrl,
				HttpStatusCode = responseStatusCode,
				ContentBytesIn = GetContentByteCount(onPremiseConnectorRequest.Body),
				ContentBytesOut = onPremiseTargetInformation.ContentBytesOut,
				OnPremiseConnectorInDate = onPremiseConnectorRequest.RequestStarted,
				OnPremiseConnectorOutDate = onPremiseConnectorRequest.RequestFinished,
				OnPremiseTargetInDate = onPremiseTargetInformation.OnPremiseTargetInDate,
				OnPremiseTargetOutDate = onPremiseTargetInformation.OnPremiseTargetOutDate,
				OriginId = Guid.Parse(originId),
				OnPremiseTargetKey = pathInformation.OnPremiseTargetKey,
				LinkId = linkId
			});
		}

		internal long GetContentByteCount(byte[] content)
		{
			if (content == null) return 0L;

			return content.LongLength;
		}

		internal OnPremiseTargetInformation GetOnPremiseTargetInformation(IOnPremiseTargetResponse onPremiseTargetResponse)
		{
			var onPremiseTargetInformation = new OnPremiseTargetInformation();

			if (onPremiseTargetResponse != null)
			{
				onPremiseTargetInformation.OnPremiseTargetInDate = onPremiseTargetResponse.RequestStarted;
				onPremiseTargetInformation.OnPremiseTargetOutDate = onPremiseTargetResponse.RequestFinished;
				onPremiseTargetInformation.ContentBytesOut = GetContentByteCount(onPremiseTargetResponse.Body);
			}

			return onPremiseTargetInformation;
		}

		internal class OnPremiseTargetInformation
		{
			public DateTime? OnPremiseTargetInDate { get; set; }
			public DateTime? OnPremiseTargetOutDate { get; set; }
			public long ContentBytesOut { get; set; }
		}
	}
}