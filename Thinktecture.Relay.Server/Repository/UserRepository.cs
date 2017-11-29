using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Serilog;
using Thinktecture.Relay.Server.Config;
using Thinktecture.Relay.Server.Dto;
using Thinktecture.Relay.Server.Repository.DbModels;
using Thinktecture.Relay.Server.Security;

namespace Thinktecture.Relay.Server.Repository
{
	public class UserRepository : IUserRepository
	{
		private readonly ILogger _logger;
		private readonly IPasswordHash _passwordHash;
		private readonly IConfiguration _configuration;

		public UserRepository(ILogger logger, IPasswordHash passwordHash, IConfiguration configuration)
		{
			_logger = logger;

			_passwordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
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
				CreationDate = DateTime.Now,
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

				if (user == null || IsUserLockedOut(user))
				{
					return null;
				}
			}

			var passwordInformation = new PasswordInformation
			{
				Hash = user.Password,
				Salt = user.Salt,
				Iterations = user.Iterations,
			};

			if (_passwordHash.ValidatePassword(Encoding.UTF8.GetBytes(password), passwordInformation))
			{
				if (user.FailedLoginAttempts.HasValue || user.LastFailedLoginAttempt.HasValue)
				{
					// login was successful, so reset potential previous failed attempts
					UnlockUser(user.Id, user.UserName);
				}

				return new User()
				{
					UserName = user.UserName,
					Id = user.Id,
					CreationDate = user.CreationDate,
				};
			}

			// We found a user, but validation failed, so record this failed attempt
			RecordFailedLoginAttempt(user);
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
					CreationDate = u.CreationDate,
				}).ToList();
			}
		}

		public bool Delete(Guid id)
		{
			using (var context = new RelayContext())
			{
				var dbUser = new DbUser
				{
					Id = id,
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
					UserName = dbUser.UserName,
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

		private bool IsUserLockedOut(DbUser user)
		{
			if (!user.FailedLoginAttempts.HasValue)
				return false;

			if (user.FailedLoginAttempts < _configuration.MaxFailedLoginAttempts)
				return false;

			// this weould be data garbage, but check nevertheless
			if (!user.LastFailedLoginAttempt.HasValue)
			{
				_logger?.Error("User {UserName} has a failed login attempt count of {FailedLoginAttempts}, but no last failed timestamp. This should not be the case.", user.UserName, user.FailedLoginAttempts);
				return true;
			}

			if (user.LastFailedLoginAttempt.Value + _configuration.FailedLoginLockoutPeriod < DateTime.UtcNow)
				return false;

			_logger?.Information("User {UserName} is locked out til {LockoutEnd} due to {FailedLoginAttempts} failed login attempts since {LastFailedLoginAttempt}",
				user.UserName,
				user.LastFailedLoginAttempt + _configuration.FailedLoginLockoutPeriod,
				user.FailedLoginAttempts,
				user.LastFailedLoginAttempt
			);

			return true;
		}

		private void RecordFailedLoginAttempt(DbUser user)
		{
			using (var ctx = new RelayContext())
			{
				var entity = new DbUser()
				{
					Id = user.Id,
				};

				ctx.Users.Attach(entity);

				entity.LastFailedLoginAttempt = DateTime.UtcNow;
				entity.FailedLoginAttempts = (user.FailedLoginAttempts ?? 0) + 1;

				ctx.Entry(entity).State = EntityState.Modified;
				ctx.SaveChanges();

				_logger?.Information("User {UserName} failed logging in. Failed attempts: {FailedLoginAttempts}",
					user.UserName,
					user.FailedLoginAttempts
				);
			}
		}

		private void UnlockUser(Guid userId, string userName)
		{
			using (var ctx = new RelayContext())
			{
				var entity = new DbUser()
				{
					Id = userId,
				};

				ctx.Users.Attach(entity);

				entity.LastFailedLoginAttempt = null;
				entity.FailedLoginAttempts = null;

				ctx.Entry(entity).State = EntityState.Modified;
				ctx.SaveChanges();

				_logger?.Information("Unlocking user account for {UserName} ", userName);
			}
		}
	}
}
