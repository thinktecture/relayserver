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
			if (connectionId == null)
				throw new ArgumentNullException(nameof(connectionId));
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			CreationDate = DateTime.UtcNow;
			_action = action;
			ConnectionId = connectionId;
		}

		public void Execute()
		{
			_action();
		}
	}
}