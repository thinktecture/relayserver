using System;

namespace Thinktecture.Relay.Server.Controller
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
