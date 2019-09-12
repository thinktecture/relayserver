using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Thinktecture.Relay.Server.Repository.DbModels
{
	[Table("Users")]
	public class DbUser
	{
		[Index(IsClustered = true)]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Identity { get; set; }

		[Key]
		public Guid Id { get; set; }

		[Required]
		[MaxLength(250)]
		[Index("UserNameIndex", IsUnique = true)]
		public string UserName { get; set; }

		[Required]
		[MaxLength(32)]
		public string Password { get; set; }

		[Required]
		[MaxLength(32)]
		public string Salt { get; set; }

		[Required]
		public int Iterations { get; set; }

		[Required]
		public DateTime CreationDate { get; set; }

		public DateTime? LastFailedLoginAttempt { get; set; }

		public int? FailedLoginAttempts { get; set; }
	}
}
