using System;

namespace Thinktecture.Relay.Server.Dto
{
	public class UpdateUser : CreateUser
	{
		public Guid Id { get; set; }
		public string PasswordOld { get; set; }
	}
}
