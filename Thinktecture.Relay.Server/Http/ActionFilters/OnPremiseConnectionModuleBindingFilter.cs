using System;

namespace Thinktecture.Relay.Server.Http.ActionFilters
{
	[AttributeUsage(AttributeTargets.Class)]
	public class OnPremiseConnectionModuleBindingFilter : CheckModuleBindingConfigurationAttribute
	{
		public OnPremiseConnectionModuleBindingFilter()
			: base(c => c.EnableOnPremiseConnections)
		{
		}
	}
}
