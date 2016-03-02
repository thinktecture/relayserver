namespace Thinktecture.Relay.Server.Dto
{
	public class PathInformation
	{
		public string CompletePath { get; set; }
		public string UserName { get; set; }
		public string OnPremiseTargetKey { get; set; }
		public string PathWithoutUserName { get; set; }
		public string LocalUrl { get; set; }
	}
}