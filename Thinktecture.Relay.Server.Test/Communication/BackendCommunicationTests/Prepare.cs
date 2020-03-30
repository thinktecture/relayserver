using System.Reactive.Subjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Thinktecture.Relay.Server.Communication.RabbitMq;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication.BackendCommunicationTests
{
	[TestClass]
	public class Prepare : BackendCommunicationTestBase
	{
		public Prepare()
		{
			CreateMocks(MockBehavior.Loose);
			PersistedSettingsMock.SetupGet(s => s.OriginId).Returns(OriginId);

			var responseSubject = new Subject<IOnPremiseConnectorResponse>();
			MessageDispatcherMock.Setup(d => d.OnResponseReceived()).Returns(responseSubject);
			var acknowledgeSubject = new Subject<IAcknowledgeRequest>();
			MessageDispatcherMock.Setup(d => d.OnAcknowledgeReceived()).Returns(acknowledgeSubject);
		}

		[TestMethod]
		public void Should_delete_all_connections()
		{
			var sut = Create();
			sut.Prepare();

			LinkRepositoryMock.Verify(r => r.DeleteAllConnectionsForOrigin(OriginId), Times.Once);
		}

		[TestMethod]
		public void Should_subscribe_to_received_responses()
		{
			var sut = Create();
			sut.Prepare();

			MessageDispatcherMock.Verify(d => d.OnResponseReceived(), Times.Once);
		}
	}
}
