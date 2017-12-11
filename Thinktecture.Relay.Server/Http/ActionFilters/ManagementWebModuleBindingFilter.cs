using System;

namespace Thinktecture.Relay.Server.Http.ActionFilters
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ManagementWebModuleBindingFilter : CheckModuleBindingConfigurationAttribute
	{
		public ManagementWebModuleBindingFilter()
			: base(c => c.EnableManagementWeb)
		{
		}
	}
}
