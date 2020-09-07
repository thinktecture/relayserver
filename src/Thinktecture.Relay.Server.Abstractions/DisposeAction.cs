using System;
using System.Threading.Tasks;

namespace Thinktecture.Relay.Server
{
	/// <summary>
	/// An implementation of a dispose action which will be executed when this instance is disposed.
	/// </summary>
	public class DisposeAction : IAsyncDisposable
	{
		private readonly Func<Task> _dispose;

		/// <summary>
		/// Initializes a new instance of <see cref="DisposeAction"/>.
		/// </summary>
		/// <param name="dispose">The asynchronous action to execute when disposing.</param>
		public DisposeAction(Func<Task> dispose)
		{
			_dispose = dispose ?? throw new ArgumentNullException(nameof(dispose));
		}

		/// <inheritdoc />
		public async ValueTask DisposeAsync() => await _dispose();
	}
}
