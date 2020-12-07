using System;
using System.Linq;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace; (extension methods on global namespace)
namespace Thinktecture.Relay
{
	/// <summary>
	/// A generic event handler for asynchronous events.
	/// </summary>
	/// <param name="sender">The sender object.</param>
	/// <param name="event">The <see cref="EventArgs"/> sent.</param>
	/// <typeparam name="T">The type of event args.</typeparam>
	public delegate Task AsyncEventHandler<in T>(object sender, T @event);

	/// <summary>
	/// Extension methods for events.
	/// </summary>
	public static class EventExtensions
	{
		/// <summary>
		/// Asynchronously invokes the event handlers.
		/// </summary>
		/// <param name="eventHandler">The <see cref="AsyncEventHandler{T}"/>.</param>
		/// <param name="sender">The sender object.</param>
		/// <param name="event">The <see cref="EventArgs"/> sent.</param>
		/// <typeparam name="T">The type of event args.</typeparam>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public static async Task InvokeAsync<T>(this AsyncEventHandler<T> eventHandler, object sender, T @event)
		{
			if (eventHandler != null)
			{
				foreach (var handler in eventHandler.GetInvocationList().Cast<AsyncEventHandler<T>>())
				{
					try
					{
						await handler(sender, @event);
					}
					catch (TaskCanceledException)
					{
						break;
					}
					catch (Exception)
					{
						// Ignore and continue with next handler
					}
				}
			}
		}
	}
}
