using System;

namespace Thinktecture.Relay.Server.Controller
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
