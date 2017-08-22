namespace Thinktecture.Relay.Server.Controller
{
	public class RelayModuleBindingFilter : CheckModuleBindingConfigurationAttribute
	{
		public RelayModuleBindingFilter()
			: base(c => c.EnableRelaying)
		{
		}
	}
}
