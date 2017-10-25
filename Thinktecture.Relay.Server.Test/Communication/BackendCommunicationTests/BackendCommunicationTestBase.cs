using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NLog;
using Thinktecture.Relay.Server.Configuration;
using Thinktecture.Relay.Server.Repository;

namespace Thinktecture.Relay.Server.Communication.BackendCommunicationTests
{
	public class BackendCommunicationTestBase
	{
		public Guid OriginId = new Guid("9DDD74B3-F8C6-4F59-A369-7E0AD3452C95");
		public Mock<IConfiguration> ConfigurationMock { get; protected set; }
		public Mock<IMessageDispatcher> MessageDispatcherMock { get; protected set; }
		public Mock<IOnPremiseConnectorCallbackFactory> RequestCallbackFactoryMock { get; protected set; }
		public Mock<ILogger> LoggerMock { get; protected set; }
		public Mock<IPersistedSettings> PersistedSettingsMock { get; protected set; }
		public Mock<ILinkRepository> LinkRepositoryMock { get; protected set; }

		protected void CreateMocks(MockBehavior mockBehavior)
		{
			ConfigurationMock = new Mock<IConfiguration>(mockBehavior);
			MessageDispatcherMock = new Mock<IMessageDispatcher>(mockBehavior);
			RequestCallbackFactoryMock = new Mock<IOnPremiseConnectorCallbackFactory>(mockBehavior);
			LoggerMock = new Mock<ILogger>(mockBehavior);
			PersistedSettingsMock = new Mock<IPersistedSettings>(mockBehavior);
			LinkRepositoryMock = new Mock<ILinkRepository>(mockBehavior);
		}

		internal BackendCommunication CreateWithMocks(MockBehavior mockBehavior)
		{
			CreateMocks(mockBehavior);
			return Create();
		}

		internal BackendCommunication Create()
		{
			return new BackendCommunication(ConfigurationMock?.Object, MessageDispatcherMock?.Object, RequestCallbackFactoryMock?.Object, LoggerMock?.Object, PersistedSettingsMock?.Object, LinkRepositoryMock?.Object);
		}
	}
}
