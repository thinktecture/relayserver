using System;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Thinktecture.Relay.Server.Owin
{
	internal class BlockNonLocalRequestsMiddleware : OwinMiddleware
	{
		private readonly string _path;

		public BlockNonLocalRequestsMiddleware(OwinMiddleware next, string path)
			: base(next)
		{
			_path = path;
		}

		public override async Task Invoke(IOwinContext context)
		{
			if (context.Request.Uri.PathAndQuery.StartsWith(_path, StringComparison.InvariantCultureIgnoreCase) && !context.Request.Uri.IsLoopback)
			{
				context.Response.StatusCode = 404;
				context.Response.Body.Flush();
			}
			else
			{
				await Next.Invoke(context).ConfigureAwait(false);
			}
		}
	}
}
