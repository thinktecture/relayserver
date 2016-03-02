using System.Net;
using System.Web.Http;
using System.Web.Http.Results;
using Thinktecture.Relay.Server.Repository;

namespace Thinktecture.Relay.Server.Controller.ManagementWeb
{
    [AllowAnonymous]
    public class SetupController : ApiController
    {
        private readonly IUserRepository _userRepository;

        public SetupController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        public IHttpActionResult NeedsFirstTimeSetup()
        {
            return _userRepository.Any() ? Ok(): (IHttpActionResult) new StatusCodeResult(HttpStatusCode.TemporaryRedirect, Request);
        }
    }
}