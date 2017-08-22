using System;

namespace Thinktecture.Relay.Server.Communication
{
	public class UnregistrationAction : IRegistryAction
	{
		private readonly Action _action;
		public DateTime CreationDate { get; }
		public bool IsRegistration => false;

		public string ConnectionId { get; }

		public UnregistrationAction(string connectionId, Action action)
		{
			CreationDate = DateTime.UtcNow;
			_action = action ?? throw new ArgumentNullException(nameof(action));
			ConnectionId = connectionId ?? throw new ArgumentNullException(nameof(connectionId));
		}

		public void Execute()
		{
			_action();
		}
	}
}
