using System.IO;

namespace Thinktecture.Relay.Server.SignalR
{
	public interface IPostDataTemporaryStore
	{
		Stream CreateRequestStream(string requestId);
		Stream GetRequestStream(string requestId);
		Stream CreateResponseStream(string requestId);
		Stream GetResponseStream(string requestId);
		long RenameResponseStream(string temporaryId, string requestId);
	}
}
