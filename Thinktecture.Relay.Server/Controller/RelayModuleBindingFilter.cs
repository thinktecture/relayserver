using System;

namespace Thinktecture.Relay.Server.Controller
{
	[AttributeUsage(AttributeTargets.Class)]
	public class RelayModuleBindingFilter : CheckModuleBindingConfigurationAttribute
	{
		public RelayModuleBindingFilter()
			: base(c => c.EnableRelaying)
		{
		}
	}
}
