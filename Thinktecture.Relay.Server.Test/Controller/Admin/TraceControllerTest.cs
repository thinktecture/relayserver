using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Thinktecture.Relay.Server.Controller.ManagementWeb;
using Thinktecture.Relay.Server.Diagnostics;
using Thinktecture.Relay.Server.Dto;
using Thinktecture.Relay.Server.Repository;
using Trace = Thinktecture.Relay.Server.Diagnostics.Trace;

namespace Thinktecture.Relay.Server.Controller.Admin
{
	// ReSharper disable JoinDeclarationAndInitializer
	// ReSharper disable PossibleNullReferenceException
	[TestClass]
	public class TraceControllerTest
	{
		[TestMethod]
		public void Create_returns_BadRequest_when_minutes_is_less_than_1()
		{
			var sut = new TraceController(null, null, null);

			var result = (BadRequestErrorMessageResult)sut.Create(new StartTrace()
			{
				LinkId = Guid.NewGuid(),
				Minutes = 0
			});

			result.Should().NotBeNull();
			result.Message.Should().Be("Tracking must be enabled for one minute at least.");
		}

		[TestMethod]
		public void Create_returns_BadRequest_when_minutes_is_greater_than_10()
		{
			var sut = new TraceController(null, null, null);

			var result = (BadRequestErrorMessageResult)sut.Create(new StartTrace()
			{
				LinkId = Guid.NewGuid(),
				Minutes = 11
			});

			result.Should().NotBeNull();
			result.Message.Should().Be("Tracking can only be enabled for ten minutes at most.");
		}

		[TestMethod]
		public void Create_returns_Created_result_with_properly_set_DTO()
		{
			var traceRepositoryMock = new Mock<ITraceRepository>();
			var sut = new TraceController(traceRepositoryMock.Object, null, null);
			CreatedNegotiatedContentResult<TraceConfiguration> result;
			var startDate = DateTime.UtcNow;
			var linkId = Guid.NewGuid();

			traceRepositoryMock.Setup(t => t.Create(It.IsAny<TraceConfiguration>()));

			result = (CreatedNegotiatedContentResult<TraceConfiguration>)sut.Create(new StartTrace()
			{
				Minutes = 5,
				LinkId = linkId
			});

			traceRepositoryMock.VerifyAll();
			result.Should().NotBeNull();
			result.Content.StartDate.Should().BeOnOrAfter(startDate).And.BeOnOrBefore(DateTime.UtcNow);
			result.Content.EndDate.Should().BeAfter(startDate.AddMinutes(5)).And.BeBefore(DateTime.UtcNow.AddMinutes(5));
			result.Content.LinkId.Should().Be(linkId);
		}

		[TestMethod]
		public void Disable_returns_OK_result_when_TraceConfiguration_was_disabled()
		{
			var traceRepositoryMock = new Mock<ITraceRepository>();
			var sut = new TraceController(traceRepositoryMock.Object, null, null);
			IHttpActionResult result;
			var traceConnectionId = Guid.NewGuid();

			traceRepositoryMock.Setup(t => t.Disable(traceConnectionId)).Returns(true);

			result = sut.Disable(traceConnectionId);

			traceRepositoryMock.VerifyAll();
			(result as OkResult)?.Should().NotBeNull();
		}

		[TestMethod]
		public void Disable_returns_BadRequest_result_when_TraceConfiguration_was_not_disabled()
		{
			var traceRepositoryMock = new Mock<ITraceRepository>();
			var sut = new TraceController(traceRepositoryMock.Object, null, null);
			IHttpActionResult result;
			var traceConnectionId = Guid.NewGuid();

			traceRepositoryMock.Setup(t => t.Disable(traceConnectionId)).Returns(false);

			result = sut.Disable(traceConnectionId);

			traceRepositoryMock.VerifyAll();
			(result as BadRequestResult)?.Should().NotBeNull();
		}

		[TestMethod]
		public void Get_returns_a_list_of_TraceConfigurations_for_the_given_linkId()
		{
			var traceRepositoryMock = new Mock<ITraceRepository>();
			var sut = new TraceController(traceRepositoryMock.Object, null, null);
			var linkId = Guid.NewGuid();
			var traceConfigurationList = new List<TraceConfiguration>();

			traceRepositoryMock.Setup(t => t.GetTraceConfigurations(linkId))
				.Returns(traceConfigurationList);

			var response = sut.Get(linkId) as OkNegotiatedContentResult<IEnumerable<TraceConfiguration>>;

			traceRepositoryMock.VerifyAll();
			response.Content.Should().BeSameAs(traceConfigurationList);
		}

		[TestMethod]
		public void IsRunning_returns_false_when_no_trace_configuration_is_available()
		{
			var traceRepositoryMock = new Mock<ITraceRepository>();
			var sut = new TraceController(traceRepositoryMock.Object, null, null);
			var connectionId = Guid.NewGuid();

			traceRepositoryMock.Setup(t => t.GetRunningTranceConfiguration(connectionId))
				.Returns(value: null);

			var response = sut.IsRunning(connectionId) as OkNegotiatedContentResult<RunningTraceConfiguration>;

			traceRepositoryMock.VerifyAll();

			response.Content.IsRunning.Should().BeFalse();
			response.Content.TraceConfiguration.Should().BeNull();
		}

		[TestMethod]
		public void IsRunning_returns_false_when_old_trace_configurations_are_available()
		{
			var traceRepositoryMock = new Mock<ITraceRepository>();
			var sut = new TraceController(traceRepositoryMock.Object, null, null);
			var connectionId = Guid.NewGuid();
			var traceConfiguration = new TraceConfiguration()
			{
				CreationDate = DateTime.Now,
				EndDate = DateTime.Now.AddMinutes(-2),
				Id = Guid.NewGuid(),
				LinkId = connectionId,
				StartDate = DateTime.Now
			};

			traceRepositoryMock.Setup(t => t.GetRunningTranceConfiguration(connectionId))
				.Returns(traceConfiguration);

			var response = sut.IsRunning(connectionId) as OkNegotiatedContentResult<RunningTraceConfiguration>;

			traceRepositoryMock.VerifyAll();

			response.Content.IsRunning.Should().BeTrue();
			response.Content.TraceConfiguration.Should().BeSameAs(traceConfiguration);
		}

		[TestMethod]
		public void IsRunning_returns_true_when_trace_is_running_and_no_old_trace_is_in_configuration_list()
		{
			var traceRepositoryMock = new Mock<ITraceRepository>();
			var sut = new TraceController(traceRepositoryMock.Object, null, null);
			var connectionId = Guid.NewGuid();
			var traceConfiguration = new TraceConfiguration()
			{
				CreationDate = DateTime.Now,
				EndDate = DateTime.Now.AddMinutes(2),
				Id = Guid.NewGuid(),
				LinkId = connectionId,
				StartDate = DateTime.Now
			};

			traceRepositoryMock.Setup(t => t.GetRunningTranceConfiguration(connectionId))
				.Returns(traceConfiguration);

			var response = sut.IsRunning(connectionId) as OkNegotiatedContentResult<RunningTraceConfiguration>;

			traceRepositoryMock.VerifyAll();

			response.Content.IsRunning.Should().BeTrue();
			response.Content.TraceConfiguration.Should().BeSameAs(traceConfiguration);
		}

		[TestMethod]
		public void GetTraceConfiguration_returns_the_correct_trace_configuration_for_given_traceConfigurationId()
		{
			var traceRepositoryMock = new Mock<ITraceRepository>();
			var sut = new TraceController(traceRepositoryMock.Object, null, null);
			var traceId = Guid.NewGuid();
			var traceConfiguration = new TraceConfiguration()
			{
				CreationDate = DateTime.Now,
				EndDate = DateTime.Now.AddMinutes(2),
				Id = traceId,
				LinkId = Guid.NewGuid(),
				StartDate = DateTime.Now
			};

			traceRepositoryMock.Setup(t => t.GetTraceConfiguration(traceId))
				.Returns(traceConfiguration);

			var response = sut.GetTraceConfiguration(traceId) as OkNegotiatedContentResult<TraceConfiguration>;

			traceRepositoryMock.VerifyAll();

			response.Content.Should().BeSameAs(traceConfiguration);
		}

		[TestMethod]
		public async Task GetFileInfoInformations_returns_the_correct_file_informations_for_given_traceConfigurationId()
		{
			var traceManagerMock = new Mock<ITraceManager>();
			var sut = new TraceController(null, traceManagerMock.Object, null);
			var traceId = Guid.NewGuid();

			var traceFiles = new Collection<Trace>()
			{
				new Trace()
				{
					OnPremiseConnectorTrace = new TraceFile()
					{
						ContentFileName = Guid.NewGuid() + "cr.content",
						HeaderFileName = Guid.NewGuid() + "cr.header",
						Headers = new Dictionary<string, string>()
						{
							["Content-Length"] = "100"
						}
					},
					OnPremiseTargetTrace = new TraceFile()
					{
						ContentFileName = Guid.NewGuid() + "ltr.content",
						HeaderFileName = Guid.NewGuid() + "ltr.header",
						Headers = new Dictionary<string, string>()
						{
							["Content-Length"] = "100"
						}
					},
					TracingDate = DateTime.Now
				}
			};

			traceManagerMock.Setup(t => t.GetTracesAsync(traceId))
				.ReturnsAsync(traceFiles);

			var response =
				await sut.GetFileInformations(traceId) as OkNegotiatedContentResult<IEnumerable<Trace>>;

			traceManagerMock.VerifyAll();

			response.Content.Should().BeSameAs(traceFiles);
		}
	}
}
