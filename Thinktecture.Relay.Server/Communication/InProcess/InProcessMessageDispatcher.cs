using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using NLog;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;

namespace Thinktecture.Relay.Server.Communication.InProcess
{
	public class InProcessMessageDispatcher : IMessageDispatcher, IDisposable
	{
		private readonly ILogger _logger;
		private readonly object _requestSubjectLookupLock;
		private readonly Dictionary<string, RequestSubjectContext> _requestSubjectLookup;
		private readonly ConcurrentDictionary<string, Subject<IOnPremiseTargetResponse>> _responseSubjectLookup;

		private bool _disposed;

		public InProcessMessageDispatcher(ILogger logger)
		{
			if (logger == null)
				throw new ArgumentNullException(nameof(logger));

			_logger = logger;
			_requestSubjectLookupLock = new object();
			_requestSubjectLookup = new Dictionary<string, RequestSubjectContext>(StringComparer.OrdinalIgnoreCase);
			_responseSubjectLookup = new ConcurrentDictionary<string, Subject<IOnPremiseTargetResponse>>(StringComparer.OrdinalIgnoreCase);
		}

		public IObservable<IOnPremiseTargetRequest> OnRequestReceived(string onPremiseId, string connectionId, bool noAck)
		{
			if (connectionId == null)
				throw new ArgumentNullException(nameof(connectionId));
			if (onPremiseId == null)
				throw new ArgumentNullException(nameof(onPremiseId));

			CheckDisposed();
			_logger.Info("Creating request subscription for OnPremiseId {0} and ConnectionId {1}", onPremiseId, connectionId);

			return Observable.Create<IOnPremiseTargetRequest>(observer =>
			{
				var ctx = GetRequestSubjectContext(onPremiseId, connectionId);
				var subscription = ctx.Subject.Subscribe(observer.OnNext);

				return new DelegatingDisposable(_logger, () =>
				{
					subscription.Dispose();

					lock (_requestSubjectLookupLock)
					{
						ctx.RemoveConnection(connectionId);

						if (ctx.ConnectionCount == 0)
							_requestSubjectLookup.Remove(onPremiseId);
					}
				});
			});
		}

		public IObservable<IOnPremiseTargetResponse> OnResponseReceived(string originId)
		{
			if (originId == null)
				throw new ArgumentNullException(nameof(originId));

			CheckDisposed();
			_logger.Info("Creating response subscription for OriginId {0}", originId);

			return Observable.Create<IOnPremiseTargetResponse>(observer =>
			{
				var subject = GetResponseSubject(originId);
				var subscription = subject.Subscribe(observer.OnNext);

				return new DelegatingDisposable(_logger, () =>
				{
					subscription.Dispose();

					Subject<IOnPremiseTargetResponse> sub;
					_responseSubjectLookup.TryRemove(originId, out sub);
				});
			});
		}

		public void AcknowledgeRequest(string onPremiseId, string acknowledgeId)
		{
			// no ack here
		}

		public Task DispatchRequest(string onPremiseId, IOnPremiseTargetRequest request)
		{
			if (onPremiseId == null)
				throw new ArgumentNullException(nameof(onPremiseId));
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			CheckDisposed();
			_logger.Debug("Dispatching request for OnPremiseId {0}. Request id: {1}, Http method {2}, Url: {3}", onPremiseId, request.RequestId, request.HttpMethod, request.Url);

			TryGetRequestSubject(onPremiseId)?.OnNext(request);

			return Task.CompletedTask;
		}

		public Task DispatchResponse(string originId, IOnPremiseTargetResponse response)
		{
			if (originId == null)
				throw new ArgumentNullException(nameof(originId));
			if (response == null)
				throw new ArgumentNullException(nameof(response));

			CheckDisposed();
			_logger.Debug("Dispatching response for OriginId {0}. Request id: {1}, Status code: {2}", originId, response.RequestId, response.StatusCode);

			GetResponseSubject(originId).OnNext(response);

			return Task.CompletedTask;
		}

		private Subject<IOnPremiseTargetRequest> TryGetRequestSubject(string onPremiseId)
		{
			lock (_requestSubjectLookupLock)
			{
				RequestSubjectContext ctx;
				if (_requestSubjectLookup.TryGetValue(onPremiseId, out ctx))
					return ctx.Subject;
			}

			return null;
		}

		private RequestSubjectContext GetRequestSubjectContext(string onPremiseId, string connectionId)
		{
			lock (_requestSubjectLookupLock)
			{
				RequestSubjectContext ctx;
				if (!_requestSubjectLookup.TryGetValue(onPremiseId, out ctx))
				{
					ctx = new RequestSubjectContext();
					_requestSubjectLookup.Add(onPremiseId, ctx);
				}

				ctx.AddConnection(connectionId);

				return ctx;
			}
		}

		private Subject<IOnPremiseTargetResponse> GetResponseSubject(string originId)
		{
			return _responseSubjectLookup.GetOrAdd(originId, id => new Subject<IOnPremiseTargetResponse>());
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
				_disposed = true
					;
				IEnumerable<IDisposable> disposables;
				lock (_requestSubjectLookupLock)
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
				_logger.Error(ex, "Error during disposing of a subject.");
			}
		}
	}
}
