using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Thinktecture.Relay.Server.Communication.BackendCommunicationTests
{
	[TestClass]
	public class Ctor : BackendCommunicationTestBase
	{
		[TestMethod]
		public void Should_create_new_instance_if_all_dependencies_are_provided()
		{
			CreateMocks(MockBehavior.Loose);
			Create();
		}

		[TestMethod]
		public void Should_set_originid()
		{
			CreateMocks(MockBehavior.Loose);
			PersistedSettingsMock.SetupGet(s => s.OriginId).Returns(OriginId);

			var sut = Create();

			sut.OriginId.Should().Be(OriginId);
		}
	}
}
