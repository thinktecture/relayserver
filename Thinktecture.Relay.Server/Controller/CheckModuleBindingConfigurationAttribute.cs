using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Thinktecture.Relay.Server.Configuration;

namespace Thinktecture.Relay.Server.Controller
{
	[AttributeUsage(AttributeTargets.Class)]
	public class CheckModuleBindingConfigurationAttribute : ActionFilterAttribute
	{
		public override bool AllowMultiple => true;

		public IConfiguration Configuration { get; set; }
		private readonly Func<IConfiguration, ModuleBinding> _getPropertyFunc;

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
