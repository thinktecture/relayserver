namespace Thinktecture.Relay.Server.Security
{
	public class PasswordInformation
	{
		public string Hash { get; set; }
		public string Salt { get; set; }
		public int Iterations { get; set; }
	}
}
