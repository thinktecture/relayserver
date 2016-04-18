using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Ploeh.AutoFixture;
using Thinktecture.Relay.Server.Dto;
using Thinktecture.Relay.Server.Repository;

namespace Thinktecture.Relay.Server.Controller.ManagementWeb
{
    [AllowAnonymous]
    public class FakeDataController : ApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly ILinkRepository _linkRepository;
        private readonly ILogRepository _logRepository;
        private readonly ITraceRepository _traceRepository;

        public FakeDataController(IUserRepository userRepository, ILinkRepository linkRepository, ILogRepository logRepository,
            ITraceRepository traceRepository)
        {
            _userRepository = userRepository;
            _linkRepository = linkRepository;
            _logRepository = logRepository;
            _traceRepository = traceRepository;
        }

        [HttpPost]
        [ActionName("create")]
        public IHttpActionResult CreateData([FromUri] int multiplier = 1)
        {
            CreateUserFakes(multiplier);
            var links = CreateLinkFakes(multiplier);
            CreateLogFakes(multiplier, links);

            return Ok();
        }

        private void CreateUserFakes(int multiplier)
        {
            var fixture = new Fixture();
            var users = fixture.Build<CreateUser>().CreateMany(10 * multiplier);

            foreach (var user in users)
            {
                _userRepository.Create(user.UserName, user.Password);
            }
        }

        private void CreateLogFakes(int multiplier, IList<Guid> links)
        {
            var random = new Random();
            var fixture = new Fixture();
            fixture.Customizations.Add(new RandomNumericSequenceGenerator(0, 10000000));
            var logs = fixture.Build<RequestLogEntry>()
                .Without(p => p.LinkId)
                .Without(p => p.OnPremiseConnectorInDate)
                .Without(p => p.OnPremiseConnectorOutDate)
                .Without(p => p.OnPremiseTargetInDate)
                .Without(p => p.OnPremiseTargetOutDate)
                .Do(logEntry =>
                {
                    logEntry.LinkId = links[random.Next(links.Count - 1)];
                    var startDate = DateTime.Now
                        .AddYears(-random.Next(0, 2))
                        .AddMonths(random.Next(0, 24) - 12)
                        .AddDays(random.Next(0, 60) - 30);
                    logEntry.OnPremiseConnectorInDate = startDate;
                    logEntry.OnPremiseConnectorOutDate = startDate.AddSeconds(random.Next(1, 120));
                    logEntry.OnPremiseTargetInDate = startDate.AddSeconds(random.Next(1, 50));
                    logEntry.OnPremiseTargetOutDate = startDate.AddSeconds(random.Next(50, 120));
                })
                .CreateMany(1000 * multiplier);

            foreach (var log in logs)
            {
                _logRepository.LogRequest(log);
            }
        }

        private IList<Guid> CreateLinkFakes(int multiplier)
        {
            var fixture = new Fixture();
            var links = fixture.Build<Link>()
                .OmitAutoProperties()
                .With(p => p.SymbolicName)
                .With(p => p.UserName)
                .CreateMany(10 * multiplier);

            return links.Select(link => _linkRepository.CreateLink(link.SymbolicName, link.UserName).Id).ToList();
        }
    }
}