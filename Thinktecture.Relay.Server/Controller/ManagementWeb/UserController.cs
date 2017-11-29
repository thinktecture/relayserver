using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System.Web.Http.Results;
using Thinktecture.Relay.Server.Dto;
using Thinktecture.Relay.Server.Http.ActionFilters;
using Thinktecture.Relay.Server.Repository;
using Thinktecture.Relay.Server.Security;

namespace Thinktecture.Relay.Server.Controller.ManagementWeb
{
	[Authorize(Roles = "Admin")]
	[ManagementWebModuleBindingFilter]
	[NoCache]
	public class UserController : ApiController
	{
		private readonly IUserRepository _userRepository;
		private readonly IPasswordComplexityValidator _passwordComplexityValidator;

		public UserController(IUserRepository userRepository, IPasswordComplexityValidator passwordComplexityValidator)
		{
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_passwordComplexityValidator = passwordComplexityValidator ?? throw new ArgumentNullException(nameof(passwordComplexityValidator));
		}

		[AllowAnonymous]
		[HttpPost]
		[ActionName("firsttime")]
		public IHttpActionResult CreateFirstUser(CreateUser user)
		{
			if (_userRepository.Any())
			{
				return new StatusCodeResult(HttpStatusCode.Forbidden, Request);
			}

			return Create(user);
		}

		[HttpGet]
		[ActionName("users")]
		public IEnumerable<User> List()
		{
			return _userRepository.List();
		}

		[HttpPost]
		[ActionName("user")]
		public IHttpActionResult Create(CreateUser user)
		{
			if (user == null)
			{
				return BadRequest();
			}

			// new password and repetition need to match
			if (user.Password != user.Password2)
			{
				return BadRequest("New password and verification do not match");
			}

			// validate password complexity by other rules
			if (!_passwordComplexityValidator.ValidatePassword(user.UserName, user.Password, out var errorMessage))
			{
				return BadRequest(errorMessage);
			}

			var id = _userRepository.Create(user.UserName, user.Password);

			if (id == Guid.Empty)
			{
				return BadRequest();
			}

			// TODO: Location
			return Created("", id);
		}

		[HttpGet]
		[ActionName("user")]
		public IHttpActionResult Get(Guid id)
		{
			var user = _userRepository.Get(id);

			if (user == null)
			{
				return BadRequest();
			}

			return Ok(user);
		}

		[HttpDelete]
		[ActionName("user")]
		public IHttpActionResult Delete(Guid id)
		{
			var result = _userRepository.Delete(id);

			return result ? (IHttpActionResult)Ok() : BadRequest();
		}

		[HttpPut]
		[ActionName("user")]
		public IHttpActionResult Update(UpdateUser user)
		{
			if (user == null)
			{
				return BadRequest();
			}

			// OldPassword needs to be correct
			var authenticatedUser = _userRepository.Authenticate(user.UserName, user.PasswordOld);
			if (authenticatedUser == null)
			{
				return BadRequest("Old password not okay");
			}

			// new password and repetition need to match
			if (user.Password != user.Password2)
			{
				return BadRequest("New password and verification do not match");
			}

			if (user.PasswordOld == user.Password)
			{
				return BadRequest("New password must be different from old one.");
			}

			// validate password complexity by other rules
			if (!_passwordComplexityValidator.ValidatePassword(authenticatedUser.UserName, user.Password, out var errorMessage))
			{
				return BadRequest(errorMessage);
			}

			var result = _userRepository.Update(authenticatedUser.Id, user.Password);

			return result ? (IHttpActionResult)Ok() : BadRequest();
		}

		[HttpGet]
		[ActionName("userNameAvailability")]
		public IHttpActionResult GetUserNameAvailability(string userName)
		{
			if (_userRepository.IsUserNameAvailable(userName))
			{
				return Ok();
			}

			return Conflict();
		}
	}
}
