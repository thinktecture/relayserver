using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Serilog;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication.InProcess
{
	internal class InProcessMessageDispatcher : IMessageDispatcher, IDisposable
	{
		private readonly ILogger _logger;
		private readonly Dictionary<Guid, RequestSubjectContext> _requestSubjectLookup;
		private readonly ConcurrentDictionary<Guid, Subject<IOnPremiseConnectorResponse>> _responseSubjectLookup;

		private bool _disposed;

		public InProcessMessageDispatcher(ILogger logger)
		{
			_logger = logger;
			_requestSubjectLookup = new Dictionary<Guid, RequestSubjectContext>();
			_responseSubjectLookup = new ConcurrentDictionary<Guid, Subject<IOnPremiseConnectorResponse>>();
		}

		public IObservable<IOnPremiseConnectorRequest> OnRequestReceived(Guid linkId, string connectionId, bool noAck)
		{
			if (connectionId == null)
				throw new ArgumentNullException(nameof(connectionId));

			CheckDisposed();
			_logger?.Information("Creating request subscription for link {0} and connection {1}", linkId, connectionId);

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

		public IObservable<IOnPremiseConnectorResponse> OnResponseReceived(Guid originId)
		{
			CheckDisposed();
			_logger?.Information("Creating response subscription");

			return Observable.Create<IOnPremiseConnectorResponse>(observer =>
			{
				var subject = GetResponseSubject(originId);
				var subscription = subject.Subscribe(observer.OnNext);

				return new DelegatingDisposable(_logger, () =>
				{
					subscription.Dispose();
					_responseSubjectLookup.TryRemove(originId, out var sub);
				});
			});
		}

		public void AcknowledgeRequest(Guid linkId, string acknowledgeId)
		{
			// no ack here
		}

		public Task DispatchRequest(Guid linkId, IOnPremiseConnectorRequest request)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			CheckDisposed();
			_logger?.Debug("Dispatching request for link {0}, request {1}, HTTP method {2}, url '{3}'", linkId, request.RequestId, request.HttpMethod, request.Url);

			TryGetRequestSubject(linkId)?.OnNext(request);

			return Task.CompletedTask;
		}

		public Task DispatchResponse(Guid originId, IOnPremiseConnectorResponse response)
		{
			if (response == null)
				throw new ArgumentNullException(nameof(response));

			CheckDisposed();
			_logger?.Debug("Dispatching response for origin {0}, request {1}, status code {2}", originId, response.RequestId, response.StatusCode);

			GetResponseSubject(originId).OnNext(response);

			return Task.CompletedTask;
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
