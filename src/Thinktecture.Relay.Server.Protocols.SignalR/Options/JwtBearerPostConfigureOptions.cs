using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace Thinktecture.Relay.Server.Protocols.SignalR.Options
{
	internal class JwtBearerPostConfigureOptions : IPostConfigureOptions<JwtBearerOptions>
	{
		public void PostConfigure(string name, JwtBearerOptions options)
		{
			// We have to hook the OnMessageReceived event in order to
			// allow the JWT authentication handler to read the access
			// token from the query string when a WebSocket or
			// Server-Sent Events request comes in.

			// Sending the access token in the query string is required due to
			// a limitation in Browser APIs. We restrict it to only calls to the
			// SignalR hub in this code.
			// See https://docs.microsoft.com/aspnet/core/signalr/security#access-token-logging
			// for more information about security considerations when using
			// the query string to transmit the access token.
			options.Events = new JwtBearerEvents()
			{
				OnMessageReceived = context =>
				{
					var accessToken = context.Request.Query["access_token"];

					// If the request is for our hub...
					var path = context.HttpContext.Request.Path;
					if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/connector")))
					{
						// Read the token out of the query string
						context.Token = accessToken;
					}

					return Task.CompletedTask;
				}
			};
		}
	}
}
