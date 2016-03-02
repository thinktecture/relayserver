using System;

namespace Thinktecture.Relay.Server.Dto
{
    public class TimeFrame
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public Resolution Resolution { get; set; }
    }
}