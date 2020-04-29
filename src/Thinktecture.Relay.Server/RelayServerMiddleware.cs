using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Thinktecture.Relay.Server
{
	public class RelayServerMiddleware
	{
		private readonly RequestDelegate _next;

		public RelayServerMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			await _next(context);
		}
	}
}
