using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Thinktecture.Relay.Server.Repository.DbModels
{
	[Table("TraceConfigurations")]
	public class DbTraceConfiguration
	{
		[Index(IsClustered = true)]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Identity { get; set; }

		[Key]
		public Guid Id { get; set; }

		[Required]
		public Guid LinkId { get; set; }

		[Required]
		public DbLink Link { get; set; }

		[Required]
		public DateTime StartDate { get; set; }

		[Required]
		public DateTime EndDate { get; set; }

		[Required]
		public DateTime CreationDate { get; set; }
	}
}
