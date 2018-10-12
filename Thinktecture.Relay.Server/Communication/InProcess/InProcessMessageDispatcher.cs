using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Serilog;
using Thinktecture.Relay.Server.Config;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication.InProcess
{
	internal class InProcessMessageDispatcher : IMessageDispatcher, IDisposable
	{
		private readonly ILogger _logger;
		private readonly Dictionary<Guid, RequestSubjectContext> _requestSubjectLookup;
		private readonly ConcurrentDictionary<Guid, Subject<IOnPremiseConnectorResponse>> _responseSubjectLookup;
		private readonly Guid _originId;

		private bool _disposed;

		public InProcessMessageDispatcher(ILogger logger, IPersistedSettings persistedSettings)
		{
			_logger = logger;
			_requestSubjectLookup = new Dictionary<Guid, RequestSubjectContext>();
			_responseSubjectLookup = new ConcurrentDictionary<Guid, Subject<IOnPremiseConnectorResponse>>();
			_originId = persistedSettings?.OriginId ?? throw new ArgumentNullException(nameof(persistedSettings));
		}

		public IObservable<IOnPremiseConnectorRequest> OnRequestReceived(Guid linkId, string connectionId, bool autoAck)
		{
			if (connectionId == null)
				throw new ArgumentNullException(nameof(connectionId));

			CheckDisposed();
			_logger?.Information("Creating request subscription for link {LinkId} and connection {ConnectionId}", linkId, connectionId);

			return Observable.Create<IOnPremiseConnectorRequest>(observer =>
			{
				var ctx = GetRequestSubjectContext(linkId, connectionId);
				var subscription = ctx.Subject.Subscribe(observer.OnNext);

				return new DelegatingDisposable(_logger, () =>
				{
					subscription.Dispose();

					lock (_requestSubjectLookup)
					{
						ctx.RemoveConnection(connectionId);

						if (ctx.ConnectionCount == 0)
							_requestSubjectLookup.Remove(linkId);
					}
				});
			});
		}

		public IObservable<IOnPremiseConnectorResponse> OnResponseReceived()
		{
			CheckDisposed();
			_logger?.Information("Creating response subscription");

			return Observable.Create<IOnPremiseConnectorResponse>(observer =>
			{
				var subject = GetResponseSubject(_originId);
				var subscription = subject.Subscribe(observer.OnNext);

				return new DelegatingDisposable(_logger, () =>
				{
					subscription.Dispose();
					_responseSubjectLookup.TryRemove(_originId, out var sub);
				});
			});
		}

		public IObservable<string> OnAcknowledgeReceived()
		{
			return Observable.Empty<string>();
		}

		public void AcknowledgeRequest(string acknowledgeId)
		{
			// no ack here
		}

		public void DispatchRequest(Guid linkId, IOnPremiseConnectorRequest request)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			CheckDisposed();
			_logger?.Debug("Dispatching request. link-id={LinkId}, request-id={RequestId}, method={HttpMethod}, url={RequestUrl}", linkId, request.RequestId, request.HttpMethod, request.Url);

			TryGetRequestSubject(linkId)?.OnNext(request);
		}

		public void DispatchResponse(Guid originId, IOnPremiseConnectorResponse response)
		{
			if (response == null)
				throw new ArgumentNullException(nameof(response));

			CheckDisposed();
			_logger?.Debug("Dispatching response. origin-id={OriginId}, request-id={RequestId}, status-code={ResponseStatusCode}", originId, response.RequestId, response.StatusCode);

			GetResponseSubject(originId).OnNext(response);
		}

		public void DispatchAcknowledge(Guid originId, string acknowledgeId)
		{
		}

		private Subject<IOnPremiseConnectorRequest> TryGetRequestSubject(Guid linkId)
		{
			lock (_requestSubjectLookup)
			{
				if (_requestSubjectLookup.TryGetValue(linkId, out var ctx))
					return ctx.Subject;
			}

			return null;
		}

		private RequestSubjectContext GetRequestSubjectContext(Guid linkId, string connectionId)
		{
			lock (_requestSubjectLookup)
			{
				if (!_requestSubjectLookup.TryGetValue(linkId, out var ctx))
				{
					ctx = new RequestSubjectContext();
					_requestSubjectLookup.Add(linkId, ctx);
				}

				ctx.AddConnection(connectionId);

				return ctx;
			}
		}

		private Subject<IOnPremiseConnectorResponse> GetResponseSubject(Guid originId)
		{
			return _responseSubjectLookup.GetOrAdd(originId, id => new Subject<IOnPremiseConnectorResponse>());
		}

		private void CheckDisposed()
		{
			if (_disposed)
				throw new ObjectDisposedException(GetType().Name);
		}

		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;

				IEnumerable<IDisposable> disposables;
				lock (_requestSubjectLookup)
				{
					disposables = _requestSubjectLookup.Values.ToArray();
				}

				DisposeSubjects(disposables);
				DisposeSubjects(_responseSubjectLookup.Values);
			}
		}

		private void DisposeSubjects(IEnumerable<IDisposable> subjects)
		{
			try
			{
				foreach (var subject in subjects)
				{
					subject.Dispose();
				}
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "Error during disposing of a subject");
			}
		}
	}
}
