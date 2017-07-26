namespace Thinktecture.Relay.Server.Controller
{
	public class ManagementWebModuleBindingFilter : CheckModuleBindingConfigurationAttribute
	{
		public ManagementWebModuleBindingFilter()
			: base(c => c.EnableManagementWeb)
		{
		}
	}
}
