using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Thinktecture.Relay.Server.Dto;
using Thinktecture.Relay.Server.Repository.DbModels;
using Thinktecture.Relay.Server.Security;

namespace Thinktecture.Relay.Server.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly IPasswordHash _passwordHash;

        public UserRepository(IPasswordHash passwordHash)
        {
            _passwordHash = passwordHash;
        }

        public Guid Create(string userName, string password)
        {
            if ((String.IsNullOrWhiteSpace(userName)) || (String.IsNullOrWhiteSpace(password)))
            {
                return Guid.Empty;
            }

            var passwordInformation = _passwordHash.CreatePasswordInformation(Encoding.UTF8.GetBytes(password));

            var dbUser = new DbUser
            {
                Id = Guid.NewGuid(),
                Iterations = passwordInformation.Iterations,
                Password = passwordInformation.Hash,
                Salt = passwordInformation.Salt,
                UserName = userName,
                CreationDate = DateTime.Now
            };

            using (var context = new RelayContext())
            {
                context.Users.Add(dbUser);
                context.SaveChanges();

                return dbUser.Id;
            }
        }

        public User Authenticate(string userName, string password)
        {
            if ((String.IsNullOrWhiteSpace(userName)) || (String.IsNullOrWhiteSpace(password)))
            {
                return null;
            }

            DbUser user;

            using (var context = new RelayContext())
            {
                user = context.Users.SingleOrDefault(u => u.UserName == userName);

                if (user == null)
                {
                    return null;
                }
            }

            var passwordInformation = new PasswordInformation
            {
                Hash = user.Password,
                Salt = user.Salt,
                Iterations = user.Iterations
            };

            if (_passwordHash.ValidatePassword(Encoding.UTF8.GetBytes(password), passwordInformation))
            {
                return new User()
                {
                    UserName = user.UserName,
                    Id = user.Id,
                    CreationDate = user.CreationDate
                };
            }

            return null;
        }

        public IEnumerable<User> List()
        {
            using (var context = new RelayContext())
            {
                return context.Users.Select(u => new User
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    CreationDate = u.CreationDate
                }).ToList();
            }
        }

        public bool Delete(Guid id)
        {
            using (var context = new RelayContext())
            {
                var dbUser = new DbUser
                {
                    Id = id
                };

                context.Users.Attach(dbUser);
                context.Users.Remove(dbUser);

                return context.SaveChanges() == 1;
            }
        }

        public bool Update(Guid id, string password)
        {
            if ((id == Guid.Empty)
                || (String.IsNullOrWhiteSpace(password)))
            {
                return false;
            }

            using (var context = new RelayContext())
            {
                var dbUser = context.Users.SingleOrDefault(u => u.Id == id);

                if (dbUser == null)
                {
                    return false;
                }

                var passwordInformation = _passwordHash.CreatePasswordInformation(Encoding.UTF8.GetBytes(password));

                dbUser.Iterations = passwordInformation.Iterations;
                dbUser.Salt = passwordInformation.Salt;
                dbUser.Password = passwordInformation.Hash;

                context.Entry(dbUser).State = EntityState.Modified;

                return context.SaveChanges() == 1;
            }
        }

        public User Get(Guid id)
        {
            using (var context = new RelayContext())
            {
                var dbUser = context.Users.SingleOrDefault(u => u.Id == id);

                if (dbUser == null)
                {
                    return null;
                }

                return new User()
                {
                    CreationDate = dbUser.CreationDate,
                    Id = dbUser.Id,
                    UserName = dbUser.UserName
                };
            }
        }

        public bool Any()
        {
            using (var context = new RelayContext())
            {
                return context.Users.Any();
            }
        }

        public bool IsUserNameAvailable(string userName)
        {
            using (var context = new RelayContext())
            {
                var user = context.Users.SingleOrDefault(u => u.UserName == userName);

                return user == null;
            }
        }
    }
}