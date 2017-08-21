using System.Collections;
using System.Collections.Generic;

namespace Thinktecture.Relay.Server.Dto
{
	public class PageResult<T>
	{
		public IEnumerable<T> Items { get; set; }
		public int Count { get; set; }
	}
}
