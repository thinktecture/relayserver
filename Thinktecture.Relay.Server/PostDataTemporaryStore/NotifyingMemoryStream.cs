using System;
using System.IO;

namespace Thinktecture.Relay.Server.PostDataTemporaryStore
{
	public class NotifyingMemoryStream : MemoryStream
	{
		public event EventHandler Disposing;

		protected override void Dispose(bool disposing)
		{
			var handler = Disposing;
			handler?.Invoke(this, EventArgs.Empty);

			base.Dispose(disposing);
		}
	}
}
