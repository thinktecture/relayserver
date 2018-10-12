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
			ConnectionId = connectionId ?? throw new ArgumentNullException(nameof(connectionId));
			_action = action ?? throw new ArgumentNullException(nameof(action));
			CreationDate = DateTime.UtcNow;
		}

		public void Execute()
		{
			_action();
		}
	}
}
