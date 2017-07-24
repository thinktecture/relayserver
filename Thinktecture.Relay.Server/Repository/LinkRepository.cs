using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Thinktecture.Relay.Server.Configuration;
using Thinktecture.Relay.Server.Dto;
using Thinktecture.Relay.Server.Repository.DbModels;
using Thinktecture.Relay.Server.Security;

namespace Thinktecture.Relay.Server.Repository
{
    public class LinkRepository : ILinkRepository
    {
        private readonly ILogger _logger;
        private readonly IPasswordHash _passwordHash;
        private readonly IConfiguration _configuration;

        private static readonly Dictionary<string, PasswordInformation> _successfullyValidatedUsernamesAndPasswords = new Dictionary<string, PasswordInformation>();

        private DateTime ActiveLinkTimeout => DateTime.UtcNow.AddSeconds(-_configuration.ActiveConnectionTimeoutInSeconds);

        public LinkRepository(ILogger logger, IPasswordHash passwordHash, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _passwordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public PageResult<Link> GetLinks(PageRequest paging)
        {
            using (var context = new RelayContext())
            {
                var query = context.Links.AsQueryable();

                if (!String.IsNullOrWhiteSpace(paging.SearchText))
                {
                    var searchText = paging.SearchText.ToLower();
                    query = query.Where(w => w.UserName.ToLower().Contains(searchText) || w.SymbolicName.ToLower().Contains(searchText));
                }

                // Default sorting must be provided
                if (String.IsNullOrWhiteSpace(paging.SortField))
                {
                    paging.SortField = "SymbolicName";
                    paging.SortDirection = SortDirection.Asc;
                }

                var count = query.Count();

                query = query.OrderByPropertyName(paging.SortField, paging.SortDirection);
                query = query.ApplyPaging(paging);

                var projection = query.Select(q => new
                    {
                        links = q,
                        connections = q.ActiveConnections.Where(ac => ac.ConnectorVersion == 0 || ac.LastActivity > ActiveLinkTimeout),
                    })
                    .ToList();

                var queryResult = projection.Select(p => GetLinkFromDbLink(p.links)).ToList();

                var result = new PageResult<Link>()
                {
                    Items = queryResult,
                    Count = count,
                };

                return result;
            }
        }

        public Link GetLink(Guid linkId)
        {
            using (var context = new RelayContext())
            {
                var projection = context.Links
                    .Where(l => l.Id == linkId)
                    .Select(l => new
                        {
                            link = l,
                            connections = l.ActiveConnections.Where(ac => ac.ConnectorVersion == 0 || ac.LastActivity > ActiveLinkTimeout),
                        })
                    .SingleOrDefault();

                if (projection == null || projection.link == null)
                {
                    return null;
                }

                return GetLinkFromDbLink(projection.link);
            }
        }

        public Link GetLink(string linkName)
        {
            using (var context = new RelayContext())
            {
                var link = context.Links.SingleOrDefault(p => p.UserName == linkName);

                if (link == null)
                {
                    return null;
                }

                return GetLinkFromDbLink(link);
            }
        }

        private Link GetLinkFromDbLink(DbLink link)
        {
            return new Link
            {
                Id = link.Id,
                CreationDate = link.CreationDate,
                ForwardOnPremiseTargetErrorResponse = link.ForwardOnPremiseTargetErrorResponse,
                IsDisabled = link.IsDisabled,
                MaximumLinks = link.MaximumLinks,
                Password = link.Password,
                SymbolicName = link.SymbolicName,
                UserName = link.UserName,
                AllowLocalClientRequestsOnly = link.AllowLocalClientRequestsOnly,
                Connections = link.ActiveConnections.Select(c => c.ConnectionId).ToList(),
                IsConnected = link.ActiveConnections.Any(),
            };
        }

        public CreateLinkResult CreateLink(string symbolicName, string userName)
        {
            using (var context = new RelayContext())
            {
                var password = _passwordHash.GeneratePassword(_configuration.LinkPasswordLength);
                var passwordInformation = _passwordHash.CreatePasswordInformation(password);

                var link = new DbLink
                {
                    Id = Guid.NewGuid(),
                    Password = passwordInformation.Hash,
                    Salt = passwordInformation.Salt,
                    Iterations = passwordInformation.Iterations,
                    SymbolicName = symbolicName,
                    UserName = userName,
                    CreationDate = DateTime.UtcNow,
                };

                context.Links.Add(link);
                context.SaveChanges();

                var result = new CreateLinkResult()
                {
                    Id = link.Id,
                    Password = Convert.ToBase64String(password),
                };

                return result;
            }
        }

        public bool UpdateLink(Link linkId)
        {
            using (var context = new RelayContext())
            {
                var itemToUpdate = context.Links.SingleOrDefault(p => p.Id == linkId.Id);

                if (itemToUpdate == null)
                {
                    return false;
                }

                itemToUpdate.CreationDate = linkId.CreationDate;
                itemToUpdate.AllowLocalClientRequestsOnly = linkId.AllowLocalClientRequestsOnly;
                itemToUpdate.ForwardOnPremiseTargetErrorResponse = linkId.ForwardOnPremiseTargetErrorResponse;
                itemToUpdate.IsDisabled = linkId.IsDisabled;
                itemToUpdate.MaximumLinks = linkId.MaximumLinks;
                itemToUpdate.SymbolicName = linkId.SymbolicName;
                itemToUpdate.UserName = linkId.UserName;

                context.Entry(itemToUpdate).State = EntityState.Modified;

                return context.SaveChanges() == 1;
            }
        }

        public void DeleteLink(Guid linkId)
        {
            using (var context = new RelayContext())
            {
                var itemToDelete = new DbLink
                {
                    Id = linkId,
                };

                context.Links.Attach(itemToDelete);
                context.Links.Remove(itemToDelete);

                context.SaveChanges();
            }
        }

        public IEnumerable<Link> GetLinks()
        {
            using (var context = new RelayContext())
            {
                return context.Links.ToList().Select(GetLinkFromDbLink).ToList();
            }
        }

        public bool Authenticate(string userName, string password, out Guid linkId)
        {
            linkId = Guid.Empty;

            using (var context = new RelayContext())
            {
                byte[] passwordBytes;

                try
                {
                    passwordBytes = Convert.FromBase64String(password);
                }
                catch
                {
                    return false;
                }

                var link = context.Links.Where(p => p.UserName == userName).Select(p => new
                {
                    p.Id,
                    p.Password,
                    p.Iterations,
                    p.Salt,
                }).FirstOrDefault();

                if (link == null)
                {
                    return false;
                }

                var passwordInformation = new PasswordInformation()
                {
                    Hash = link.Password,
                    Iterations = link.Iterations,
                    Salt = link.Salt,
                };

                var cacheKey = userName + "/" + password;
                PasswordInformation previousInfo = null;

                lock (_successfullyValidatedUsernamesAndPasswords)
                {
                    _successfullyValidatedUsernamesAndPasswords.TryGetValue(cacheKey, out previousInfo);
                }

                // found in cache (NOTE: cache only contains successfully validated passwords to prevent DOS attacks!)
                if (previousInfo != null)
                {
                    if (previousInfo.Hash == passwordInformation.Hash &&
                        previousInfo.Iterations == passwordInformation.Iterations &&
                        previousInfo.Salt == passwordInformation.Salt)
                    {
                        linkId = link.Id;
                        return true;
                    }
                }

                // ELSE: calculate and cache
                if (!_passwordHash.ValidatePassword(passwordBytes, passwordInformation))
                {
                    return false;
                }

                lock (_successfullyValidatedUsernamesAndPasswords)
                {
                    {
                        _successfullyValidatedUsernamesAndPasswords[cacheKey] = passwordInformation;
                    }
                }

                linkId = link.Id;
                return true;
            }
        }

        public bool IsUserNameAvailable(string userName)
        {
            using (var context = new RelayContext())
            {
                return !context.Links.Any(p => p.UserName == userName);
            }
        }

        public Task AddOrRenewActiveConnection(string linkId, string originId, string connectionId, int connectorVersion)
        {
            _logger.Trace("Adding or updating connection {0} for link {1} and origin {2} with connector version {3}", connectionId, linkId, originId, connectorVersion);

            return Task.Run(() =>
            {
                AddOrRenewActiveConnection(new Guid(linkId), new Guid(originId), connectionId, connectorVersion);
            });
        }

        private void AddOrRenewActiveConnection(Guid linkId, Guid originId, string connectionId, int connectorVersion)
        {
            using (var context = new RelayContext())
            {
                var activeConnection = context.ActiveConnections.FirstOrDefault(ac => ac.LinkId == linkId && ac.OriginId == originId && ac.ConnectionId == connectionId);

                if (activeConnection != null)
                {
                    activeConnection.LastActivity = DateTime.UtcNow;
                }
                else
                {
                    context.ActiveConnections.Add(new DbActiveConnection()
                    {
                        LinkId = linkId,
                        OriginId = originId,
                        ConnectionId = connectionId,
                        ConnectorVersion = connectorVersion,
                        LastActivity = DateTime.UtcNow,
                    });
                }

                context.SaveChanges();
            }
        }

        public Task RenewActiveConnection(string connectionId)
        {
            _logger.Trace("Renewing last activity on connection {0}", connectionId);

            return Task.Run(() =>
            {
                RenewActiveConnectionInternal(connectionId);
            });
        }

        private void RenewActiveConnectionInternal(string connectionId)
        {
            using (var context = new RelayContext())
            {
                var activeConnection = context.ActiveConnections.FirstOrDefault(ac => ac.ConnectionId == connectionId);

                if (activeConnection != null)
                {
                    activeConnection.LastActivity = DateTime.UtcNow;
                    context.SaveChanges();
                }
            }
        }

        public Task RemoveActiveConnection(string connectionId)
        {
            _logger.Debug("Deleting active connection {0}", connectionId);
            return Task.Run(() =>
            {
                using (var context = new RelayContext())
                {
                    var activeConnection = context.ActiveConnections.FirstOrDefault(ac => ac.ConnectionId == connectionId);

                    if (activeConnection != null)
                    {
                        context.ActiveConnections.Remove(activeConnection);
                        context.SaveChanges();
                    }
                }
            });
        }

        public void DeleteAllActiveConnectionsForOrigin(string originId)
        {
            _logger.Debug("Deleting all active connections for Origin {0}", originId);
            DeleteAllActiveConnectionsForOrigin(new Guid(originId));
        }

        private void DeleteAllActiveConnectionsForOrigin(Guid originId)
        {
            using (var context = new RelayContext())
            {
                var invalidConnections = context.ActiveConnections.Where(ac => ac.OriginId == originId);
                context.ActiveConnections.RemoveRange(invalidConnections);

                context.SaveChanges();
            }
        }
    }
}
