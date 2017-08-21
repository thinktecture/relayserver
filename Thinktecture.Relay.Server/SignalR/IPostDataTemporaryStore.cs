namespace Thinktecture.Relay.Server.SignalR
{
	public interface IPostDataTemporaryStore
	{
		void Save(string requestId, byte[] data);
		byte[] Load(string requestId);
	}
}
