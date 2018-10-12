using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Thinktecture.Relay.Server.Repository.DbModels
{
	[Table("ActiveConnections")]
	internal class DbActiveConnection
	{
		[Required]
		public Guid LinkId { get; set; }

		[Required]
		public string ConnectionId { get; set; }

		[Required]
		public int ConnectorVersion { get; set; }

		[Required]
		public string AssemblyVersion { get; set; }

		[Required]
		public Guid OriginId { get; set; }

		[Required]
		public DateTime LastActivity { get; set; }

		public virtual DbLink Link { get; set; }
	}
}
