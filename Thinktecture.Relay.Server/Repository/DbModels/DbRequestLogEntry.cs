using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace Thinktecture.Relay.Server.Repository.DbModels
{
	[Table("RequestLogEntries")]
	public class DbRequestLogEntry
	{
		[Index(IsClustered = true)]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Identity { get; set; }

		[Key]
		public Guid Id { get; set; }

		[Required]
		public Guid OriginId { get; set; }

		[Required]
		[Index("IX_HttpStatusCode")]
		public HttpStatusCode HttpStatusCode { get; set; }

		[Required]
		public DbLink Link { get; set; }

		[Required]
		public Guid LinkId { get; set; }

		[Required]
		[MaxLength(250)]
		[Index("IX_OnPremiseTargetKey")]
		public string OnPremiseTargetKey { get; set; }

		[Required]
		[MaxLength(250)]
		[Index("IX_LocalUrl")]
		public string LocalUrl { get; set; }

		[Required]
		[Index("IX_ContentBytesIn")]
		public long ContentBytesIn { get; set; }

		[Required]
		[Index("IX_ContentBytesOut")]
		public long ContentBytesOut { get; set; }

		[Required]
		[Index("IX_OnPremiseConnectorInDate")]
		public DateTime OnPremiseConnectorInDate { get; set; }

		[Required]
		[Index("IX_OnPremiseConnectorOutDate")]
		public DateTime OnPremiseConnectorOutDate { get; set; }

		[Index("IX_OnPremiseTargetInDate")]
		public DateTime? OnPremiseTargetInDate { get; set; }

		[Index("IX_OnPremiseTargetOutDate")]
		public DateTime? OnPremiseTargetOutDate { get; set; }

		[MaxLength(36)]
		[Index("IX_RequestId")]
		public string RequestId { get; set; }
	}
}
