using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System.Web.Http.Results;
using Thinktecture.Relay.Server.Dto;
using Thinktecture.Relay.Server.Http.Filters;
using Thinktecture.Relay.Server.Repository;

namespace Thinktecture.Relay.Server.Controller.ManagementWeb
{
	[Authorize(Roles = "Admin")]
	[ManagementWebModuleBindingFilter]
	[NoCache]
	public class UserController : ApiController
	{
		private readonly IUserRepository _userRepository;

		public UserController(IUserRepository userRepository)
		{
			_userRepository = userRepository;
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
			if (_userRepository.Authenticate(user.UserName, user.PasswordOld) == null)
			{
				return BadRequest("Old password not okay");
			}

			// new password and repetition need to match
			if (user.Password != user.Password2)
			{
				return BadRequest("New password and verification do not match");
			}

			var result = _userRepository.Update(user.Id, user.Password);

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
