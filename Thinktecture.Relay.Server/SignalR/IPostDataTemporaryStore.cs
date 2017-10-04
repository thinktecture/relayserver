namespace Thinktecture.Relay.Server.SignalR
{
	public interface IPostDataTemporaryStore
	{
		void SaveRequest(string requestId, byte[] data);
		void SaveResponse(string requestId, byte[] data);
		byte[] LoadRequest(string requestId);
		byte[] LoadResponse(string requestId);
	}
}
