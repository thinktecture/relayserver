using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Thinktecture.Relay.Server.Configuration;
using Thinktecture.Relay.Server.Dto;
using Thinktecture.Relay.Server.Repository.DbModels;
using Thinktecture.Relay.Server.Security;

namespace Thinktecture.Relay.Server.Repository
{
    public class LinkRepository : ILinkRepository
    {
        private readonly IPasswordHash _passwordHash;
        private readonly IConfiguration _configuration;

        private static readonly Dictionary<string, PasswordInformation> _successfullyValidatedUsernamesAndPasswords = new Dictionary<string, PasswordInformation>();

        public LinkRepository(IPasswordHash passwordHash, IConfiguration configuration)
        {
            _passwordHash = passwordHash;
            _configuration = configuration;
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

                var queryResult = query.ToList().Select(GetLinkFromDbLink).ToList();

                var result = new PageResult<Link>()
                {
                    Items = queryResult,
                    Count = count
                };

                return result;
            }
        }

        public Link GetLink(Guid linkId)
        {
            using (var context = new RelayContext())
            {
                var link = context.Links.SingleOrDefault(p => p.Id == linkId);

                if (link == null)
                {
                    return null;
                }

                return GetLinkFromDbLink(link);
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
                AllowLocalClientRequestsOnly = link.AllowLocalClientRequestsOnly
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
                    CreationDate = DateTime.UtcNow
                };

                context.Links.Add(link);
                context.SaveChanges();

                var result = new CreateLinkResult()
                {
                    Id = link.Id,
                    Password = Convert.ToBase64String(password)
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
                    Id = linkId
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
                    p.Salt
                }).FirstOrDefault();

                if (link == null)
                {
                    return false;
                }

                var passwordInformation = new PasswordInformation()
                {
                    Hash = link.Password,
                    Iterations = link.Iterations,
                    Salt = link.Salt
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
    }
}