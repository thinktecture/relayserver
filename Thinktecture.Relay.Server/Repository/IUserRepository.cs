using System;
using System.Collections.Generic;
using Thinktecture.Relay.Server.Dto;

namespace Thinktecture.Relay.Server.Repository
{
    public interface IUserRepository
    {
        Guid Create(string userName, string password);
        User Authenticate(string userName, string password);
        IEnumerable<User> List();
        bool Delete(Guid id);
        bool Update(Guid id, string password);
        User Get(Guid id);
        bool Any();
	    bool IsUserNameAvailable(string userName);
    }
}