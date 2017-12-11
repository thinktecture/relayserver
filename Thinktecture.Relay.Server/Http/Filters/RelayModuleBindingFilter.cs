using System;

namespace Thinktecture.Relay.Server.Http.Filters
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
