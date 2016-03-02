using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLog.Interface;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication
{
	internal abstract class BackendCommunication : IBackendCommunication
	{
		private bool _disposed;

		public string OriginId { get; private set; }

		public abstract Task<IOnPremiseTargetReponse> GetResponseAsync(string requestId);
		public abstract Task SendOnPremiseConnectorRequest(string onPremiseId, IOnPremiseConnectorRequest onPremiseConnectorRequest);
		public abstract void RegisterOnPremise(RegistrationInformation registrationInformation);
		public abstract void UnregisterOnPremise(string connectionId);
		public abstract Task SendOnPremiseTargetResponse(string originId, IOnPremiseTargetReponse reponse);
	    public abstract bool IsRegistered(string connectionId);
	    public abstract List<string> GetConnections(string linkId);

	    protected BackendCommunication(ILogger logger)
		{
            OriginId = Guid.NewGuid().ToString();
            logger.Trace("Creating backend communication with origin id {0}", OriginId);
		}

		protected void CheckDisposed()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}

		#region IDisposable

		~BackendCommunication()
		{
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
	}
}
