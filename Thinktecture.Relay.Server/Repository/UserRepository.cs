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

			using (var context = new RelayContext())
			{
				var user = context.Users.SingleOrDefault(u => u.UserName == userName);

				if (user == null || IsUserLockedOut(user))
				{
					return null;
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
						UnlockUser(context, user);
					}

					return new User()
					{
						UserName = user.UserName,
						Id = user.Id,
						CreationDate = user.CreationDate,
					};
				}

				// We found a user, but validation failed, so record this failed attempt
				RecordFailedLoginAttempt(context, user);
				return null;
			}
		}

		public IEnumerable<User> List()
		{
			using (var context = new RelayContext())
			{
				return context.Users.Select(u => new
				{
					u.Id,
					u.UserName,
					u.CreationDate,
					u.FailedLoginAttempts,
					u.LastFailedLoginAttempt,
				})
				.ToList()
				.Select(u => new User()
				{
					Id = u.Id,
					UserName = u.UserName,
					CreationDate = u.CreationDate,
					LockedUntil = LockedOutUntil(u.FailedLoginAttempts, u.LastFailedLoginAttempt),
				});
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

		private DateTime? LockedOutUntil(int? failedAttempts, DateTime? lastFailedAttempt)
		{
			if (!failedAttempts.HasValue)
				return null;

			if (failedAttempts < _configuration.MaxFailedLoginAttempts)
				return null;

			if (!lastFailedAttempt.HasValue)
				return null;

			var result = lastFailedAttempt + _configuration.FailedLoginLockoutPeriod;
			if (result < DateTime.UtcNow)
				return null;

			return result;
		}

		private bool IsUserLockedOut(DbUser user)
		{
			var lockedOutUntil = LockedOutUntil(user.FailedLoginAttempts, user.LastFailedLoginAttempt);
			if (!lockedOutUntil.HasValue)
				return false;

			_logger?.Information("User {UserName} is locked out til {LockoutEnd} due to {FailedLoginAttempts} failed login attempts since {LastFailedLoginAttempt}",
				user.UserName,
				lockedOutUntil,
				user.FailedLoginAttempts,
				user.LastFailedLoginAttempt
			);

			return true;
		}

		private void RecordFailedLoginAttempt(RelayContext ctx, DbUser user)
		{
			user.LastFailedLoginAttempt = DateTime.UtcNow;
			user.FailedLoginAttempts = user.FailedLoginAttempts.GetValueOrDefault() + 1;

			ctx.Entry(user).State = EntityState.Modified;
			ctx.SaveChanges();

			_logger?.Information("User {UserName} failed logging in. Failed attempts: {FailedLoginAttempts}",
				user.UserName,
				user.FailedLoginAttempts
			);
		}

		private void UnlockUser(RelayContext ctx, DbUser user)
		{
			user.LastFailedLoginAttempt = null;
			user.FailedLoginAttempts = null;

			ctx.Entry(user).State = EntityState.Modified;
			ctx.SaveChanges();

			_logger?.Information("Unlocking user account for {UserName}", user.UserName);
		}
	}
}
