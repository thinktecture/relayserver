using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Thinktecture.Relay.Server.Config;

namespace Thinktecture.Relay.Server.Http.Filters
{
	[AttributeUsage(AttributeTargets.Class)]
	public class CheckModuleBindingConfigurationAttribute : ActionFilterAttribute
	{
		private readonly Func<IConfiguration, ModuleBinding> _getPropertyFunc;

		public override bool AllowMultiple => true;

		// This will be set by Autofac property injection (hooked up in startup)
		public IConfiguration Configuration { get; set; }

		public CheckModuleBindingConfigurationAttribute(Func<IConfiguration, ModuleBinding> getPropertyFunc)
		{
			_getPropertyFunc = getPropertyFunc;
		}

		public override void OnActionExecuting(HttpActionContext actionContext)
		{
			var value = _getPropertyFunc(Configuration);

			if ((value == ModuleBinding.False) || (value == ModuleBinding.Local && !actionContext.Request.IsLocal()))
			{
				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound));
			}
		}
	}
}
