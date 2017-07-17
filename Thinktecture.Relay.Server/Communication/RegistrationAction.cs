using System;

namespace Thinktecture.Relay.Server.Communication
{
	public class RegistrationAction : IRegistryAction
	{
		private readonly Action _action;
		public DateTime CreationDate { get; }
		public bool IsRegistration => true;

		public string ConnectionId { get; }

		public RegistrationAction(string connectionId, Action action)
		{
			if (connectionId == null)
				throw new ArgumentNullException(nameof(connectionId));
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			CreationDate = DateTime.UtcNow;
			ConnectionId = connectionId;
			_action = action;
		}

		public void Execute()
		{
			_action();
		}
	}
}