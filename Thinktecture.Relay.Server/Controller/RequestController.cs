using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using NLog.Interface;
using Thinktecture.Relay.Server.SignalR;

namespace Thinktecture.Relay.Server.Controller
{
    public class RequestController : ApiController
    {
        private readonly IPostDataTemporaryStore _temporaryStore;
        private readonly ILogger _logger;

        public RequestController(IPostDataTemporaryStore temporaryStore, ILogger logger)
        {
            _temporaryStore = temporaryStore;
            _logger = logger;
        }

        public IHttpActionResult Get(string requestId)
        {
            _logger.Trace("Getting request {0}", requestId);

            var data = _temporaryStore.Load(requestId);
            if (data.Length == 0)
            {
                _logger.Info("No data found for request id {0}", requestId);
                return NotFound();
            }

            return new ResponseMessageResult(new HttpResponseMessage() { Content = new ByteArrayContent(data) });
        }
    }
}
