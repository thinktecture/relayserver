using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace Thinktecture.Relay.Server.Repository.DbModels
{
    [Table("RequestLogEntries")]
    internal class DbRequestLogEntry
    {
        [Index(IsClustered = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Identity { get; set; }

        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid OriginId { get; set; }

        [Required]
        public HttpStatusCode HttpStatusCode { get; set; }

        [Required]
        public DbLink Link { get; set; }

        [Required]
        public Guid LinkId { get; set; }

        [Required]
        public string OnPremiseTargetKey { get; set; }

        [Required]
        public string LocalUrl { get; set; }

        [Required]
        public long ContentBytesIn { get; set; }

        [Required]
        public long ContentBytesOut { get; set; }

        [Required]
        public DateTime OnPremiseConnectorInDate { get; set; }

        [Required]
        public DateTime OnPremiseConnectorOutDate { get; set; }

        public DateTime? OnPremiseTargetInDate { get; set; }
        public DateTime? OnPremiseTargetOutDate { get; set; }
    }
}