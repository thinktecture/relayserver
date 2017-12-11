using System;
using Owin;

namespace Thinktecture.Relay.Server.Owin
{
	public static class HstsAppBuilderExtensions
	{
		/// <summary>
		/// Enables HTTP Strict Transport Security (HSTS) for the hosting application.
		/// </summary>
		/// <param name="app">The application.</param>
		/// <param name="duration">The duration the HSTS header should be cached in the client browser. <c>TimeSpan.Zero</c> will clear the cached value.</param>
		/// <param name="includeSubdomains">Determines whether the includeSubdomains directive should be added. Defaults to true.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">app</exception>
		/// <exception cref="System.ArgumentException">duration cannot be below zero</exception>
		public static IAppBuilder UseHsts(this IAppBuilder app, TimeSpan duration, bool includeSubdomains = true)
		{
			if (app == null) throw new ArgumentNullException(nameof(app));
			if (duration < TimeSpan.Zero) throw new ArgumentException("duration cannot be below zero");

			string headerValue = $"max-age={(int)duration.TotalSeconds}" + (includeSubdomains ? "; includeSubdomains" : String.Empty);

			app.Use(async (ctx, next) =>
			{
				if (ctx.Request.IsSecure)
				{
					ctx.Response.Headers.Append("Strict-Transport-Security", headerValue);
				}

				await next();
			});

			return app;
		}

		/// <summary>
		/// Enables HTTP Strict Transport Security (HSTS) for the hosting application.
		/// </summary>
		/// <param name="app">The application.</param>
		/// <param name="days">The number of days the HSTS header should be cached in the client browser. A value of zero will clear the cached value.</param>
		/// <param name="includeSubdomains">Determines whether the includeSubdomains directive should be added. Defaults to true.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentException">days cannot be below zero</exception>
		/// <exception cref="System.ArgumentNullException">app</exception>
		public static IAppBuilder UseHsts(this IAppBuilder app, int days = 365, bool includeSubdomains = true)
		{
			if (days < 0) throw new ArgumentException("days cannot be below zero");

			return app.UseHsts(TimeSpan.FromDays(days), includeSubdomains);
		}
	}
}
