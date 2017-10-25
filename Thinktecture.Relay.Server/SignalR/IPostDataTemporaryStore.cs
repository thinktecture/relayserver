using System.IO;

namespace Thinktecture.Relay.Server.SignalR
{
	public interface IPostDataTemporaryStore
	{
		void SaveRequest(string requestId, byte[] data);
		void SaveResponse(string requestId, byte[] data);
		Stream CreateResponseStream(string requestId);
		Stream GetResponseStream(string requestId);
		byte[] LoadRequest(string requestId);
		byte[] LoadResponse(string requestId);
		Stream CreateRequestStream(string requestId);
		Stream GetRequestStream(string requestId);
	}
}
