using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Thinktecture.Relay.Server.Repository.DbModels
{
	[Table("Links")]
	internal class DbLink
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
		public string Password { get; set; }

		[Required]
		public int Iterations { get; set; }

		[Required]
		public string Salt { get; set; }

		[Required]
		[MaxLength(250)]
		public string SymbolicName { get; set; }

		[Required]
		public bool IsDisabled { get; set; }

		[Required]
		public bool ForwardOnPremiseTargetErrorResponse { get; set; }

		[Required]
		public bool AllowLocalClientRequestsOnly { get; set; }

		[Required]
		public int MaximumLinks { get; set; }

		[Required]
		public DateTime CreationDate { get; set; }

		public TimeSpan? TokenRefreshWindow { get; set; }
		public TimeSpan? HeartbeatInterval { get; set; }
		public TimeSpan? ReconnectMinWaitTime { get; set; }
		public TimeSpan? ReconnectMaxWaitTime { get; set; }

		public TimeSpan? AbsoluteConnectionLifetime { get; set; }
		public TimeSpan? SlidingConnectionLifetime { get; set; }

		private ICollection<DbActiveConnection> _activeConnections;

		public virtual ICollection<DbActiveConnection> ActiveConnections
		{
			get => _activeConnections ?? (_activeConnections = new List<DbActiveConnection>());
			set => _activeConnections = value;
		}
	}
}
