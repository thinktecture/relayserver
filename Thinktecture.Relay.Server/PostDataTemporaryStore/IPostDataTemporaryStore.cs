using System.IO;

namespace Thinktecture.Relay.Server.PostDataTemporaryStore
{
	public interface IPostDataTemporaryStore
	{
		Stream CreateRequestStream(string requestId);
		Stream GetRequestStream(string requestId);
		Stream CreateResponseStream(string requestId);
		Stream GetResponseStream(string requestId);
	}
}
