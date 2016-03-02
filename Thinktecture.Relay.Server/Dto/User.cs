using System;

namespace Thinktecture.Relay.Server.Dto
{
    public class User
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public DateTime CreationDate { get; set; }
    }
}