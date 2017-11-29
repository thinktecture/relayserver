using System;

namespace Thinktecture.Relay.Server.Http.ActionFilters
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
