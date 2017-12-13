using System.Web.Http;
using System.Web.Http.Controllers;
using Thinktecture.Relay.Server.Config;

namespace Thinktecture.Relay.Server.Filters
{
	public class ConfigurableClientAuthorizationAttribute : AuthorizeAttribute
	{
		// This will be set by Autofac property injection (hooked up in startup)
		public IConfiguration Configuration { get; set; }

		protected override bool IsAuthorized(HttpActionContext actionContext)
		{
			if (!Configuration.SecureClientController)
				return true;

			return base.IsAuthorized(actionContext);
		}
	}
}
