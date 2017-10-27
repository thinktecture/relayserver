using System.IO;

namespace Thinktecture.Relay.Server.SignalR
{
	public interface IPostDataTemporaryStore
	{
		byte[] LoadRequest(string requestId);
		Stream CreateRequestStream(string requestId);
		Stream GetRequestStream(string requestId);
		void SaveResponse(string requestId, byte[] data);
		Stream CreateResponseStream(string requestId);
		Stream GetResponseStream(string requestId);
	}
}
