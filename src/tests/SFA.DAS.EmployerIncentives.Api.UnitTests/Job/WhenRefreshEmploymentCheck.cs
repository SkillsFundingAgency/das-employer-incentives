using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.Job
{
    [TestFixture]
    public class WhenRefreshEmploymentCheck
    {
        private JobCommandController _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            _fixture = new Fixture();
            _sut = new JobCommandController(_mockCommandDispatcher.Object);

            _mockCommandDispatcher
                .Setup(m => m.Send(It.IsAny<RefreshEmploymentCheckCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Test]
        public async Task Then_a_refresh_employment_check_command_is_dispatched()
        {
            // Arrange
            var serviceRequest = _fixture.Create<ServiceRequest>();
            var accountLegalEntityId = _fixture.Create<long>();
            var uln = _fixture.Create<long>();
            var checkType = RefreshEmploymentCheckType.InitialEmploymentChecks.ToString();
            var applications = new List<Types.Application>
            {
                new Types.Application { AccountLegalEntityId = accountLegalEntityId, ULN = uln }
            };
            var data = new Dictionary<string, object>
            {
                { "CheckType", checkType },
                { "Applications", JsonConvert.SerializeObject(applications) },
                { "ServiceRequest", JsonConvert.SerializeObject(serviceRequest) }
            };

            var request = _fixture.Build<JobRequest>()
                    .With(r => r.Type, JobType.RefreshEmploymentChecks)
                    .With(r => r.Data, data)
                    .Create();

            RefreshEmploymentCheckCommand sentCommand = null;

            _mockCommandDispatcher
                .Setup(m => m.SendMany(It.IsAny<IEnumerable<ICommand>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<ICommand>, CancellationToken>((c, t) =>
                {
                    sentCommand = c.Single() as RefreshEmploymentCheckCommand;
                });

            // Act
            await _sut.AddJob(request);

            // Assert
            _mockCommandDispatcher.Verify(m => m.SendMany(It.IsAny<IEnumerable<RefreshEmploymentCheckCommand>>(), It.IsAny<CancellationToken>()), Times.Once);
            sentCommand.ULN.Should().Be(uln);
            sentCommand.AccountLegalEntityId.Should().Be(accountLegalEntityId);
            sentCommand.ServiceRequestTaskId.Should().Be(serviceRequest.TaskId);
            sentCommand.DecisionReference.Should().Be(serviceRequest.DecisionReference);
            sentCommand.ServiceRequestCreated.Should().Be(serviceRequest.TaskCreatedDate.Value);
        }

        [Test]
        public async Task Then_a_NoContent_response_is_returned()
        {
            // Arrange
            var serviceRequest = _fixture.Create<ServiceRequest>();
            var accountLegalEntityId = _fixture.Create<long>();
            var uln = _fixture.Create<long>();
            var checkType = RefreshEmploymentCheckType.EmployedAt365DaysCheck.ToString();
            var applications = new List<Types.Application>
            {
                new Types.Application { AccountLegalEntityId = accountLegalEntityId, ULN = uln }
            };
            var data = new Dictionary<string, object>
            {
                { "CheckType", checkType },
                { "Applications", JsonConvert.SerializeObject(applications) },
                { "ServiceRequest", JsonConvert.SerializeObject(serviceRequest) }
            };

            var request = _fixture.Build<JobRequest>()
                    .With(r => r.Type, JobType.RefreshEmploymentChecks)
                    .With(r => r.Data, data)
                    .Create();

            // Act
            var actual = await _sut.AddJob(request) as NoContentResult;

            // Assert
            actual.Should().NotBeNull();
        }
    }
}
