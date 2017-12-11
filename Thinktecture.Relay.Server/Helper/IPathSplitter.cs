using Thinktecture.Relay.Server.Dto;

namespace Thinktecture.Relay.Server.Helper
{
	public interface IPathSplitter
	{
		PathInformation Split(string path);
	}
}
