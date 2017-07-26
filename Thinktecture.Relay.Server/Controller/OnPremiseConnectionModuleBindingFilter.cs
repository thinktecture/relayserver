namespace Thinktecture.Relay.Server.Controller
{
	public class OnPremiseConnectionModuleBindingFilter : CheckModuleBindingConfigurationAttribute
	{
		public OnPremiseConnectionModuleBindingFilter()
			: base(c => c.EnableOnPremiseConnections)
		{
		}
	}
}
