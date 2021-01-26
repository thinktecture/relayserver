using System;
using System.Collections.Generic;
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
		/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the raised exceptions during the invocation.</returns>
		public static async Task<Exception[]> InvokeAsync<T>(this AsyncEventHandler<T> eventHandler, object sender, T @event)
		{
			if (eventHandler == null) return Array.Empty<Exception>();

			var exceptions = new List<Exception>();
			foreach (var handler in eventHandler.GetInvocationList().Cast<AsyncEventHandler<T>>())
			{
				try
				{
					await handler(sender, @event);
				}
				catch (OperationCanceledException)
				{
					break;
				}
				catch (Exception ex)
				{
					exceptions.Add(ex);
				}
			}

			return exceptions.ToArray();
		}
	}
}
