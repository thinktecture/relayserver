using System;
using System.Net.Http.Headers;
using System.Web.Http.Filters;

namespace Thinktecture.Relay.Server.Http.Filters
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class NoCacheAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
		{
			if (actionExecutedContext.Response != null)
			{
				actionExecutedContext.Response.Headers.CacheControl = new CacheControlHeaderValue()
				{
					NoCache = true,
					NoStore = true,
				};
			}

			base.OnActionExecuted(actionExecutedContext);
		}
	}
}
