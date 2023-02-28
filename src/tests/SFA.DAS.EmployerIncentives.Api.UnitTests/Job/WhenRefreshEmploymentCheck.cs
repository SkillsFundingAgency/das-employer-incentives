using System;
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
using System.Net;
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
                .Setup(m => m.SendMany(It.IsAny<IEnumerable<RefreshEmploymentCheckCommand>>(), It.IsAny<CancellationToken>()))
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
            var requests = new List<EmploymentCheckRefreshRequest>
            {
                new EmploymentCheckRefreshRequest
                {
                    Applications = applications.ToArray(),
                    ServiceRequest = serviceRequest,
                    CheckType = checkType
                }
            };
            var data = new Dictionary<string, object>
            {
                { "Requests", JsonConvert.SerializeObject(requests) }
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
        public async Task Then_multiple_refresh_employment_check_commands_are_dispatched()
        {
            // Arrange
            var serviceRequest1 = _fixture.Create<ServiceRequest>();
            var serviceRequest2 = _fixture.Create<ServiceRequest>();
            var accountLegalEntityId1 = _fixture.Create<long>();
            var accountLegalEntityId2 = _fixture.Create<long>();
            var uln1 = _fixture.Create<long>();
            var uln2 = _fixture.Create<long>();
            var checkType1 = RefreshEmploymentCheckType.InitialEmploymentChecks.ToString();
            var checkType2 = RefreshEmploymentCheckType.EmployedAt365DaysCheck.ToString();
            var applications1 = new List<Types.Application>
            {
                new Types.Application { AccountLegalEntityId = accountLegalEntityId1, ULN = uln1 }
            };
            var applications2 = new List<Types.Application>
            {
                new Types.Application { AccountLegalEntityId = accountLegalEntityId2, ULN = uln2 }
            };
            var requests = new List<EmploymentCheckRefreshRequest>
            {
                new EmploymentCheckRefreshRequest
                {
                    Applications = applications1.ToArray(),
                    ServiceRequest = serviceRequest1,
                    CheckType = checkType1
                },
                new EmploymentCheckRefreshRequest
                {
                    Applications = applications2.ToArray(),
                    ServiceRequest = serviceRequest2,
                    CheckType = checkType2
                }
            };
            var data = new Dictionary<string, object>
            {
                { "Requests", JsonConvert.SerializeObject(requests) }
            };

            var request = _fixture.Build<JobRequest>()
                    .With(r => r.Type, JobType.RefreshEmploymentChecks)
                    .With(r => r.Data, data)
                    .Create();

            RefreshEmploymentCheckCommand firstCommand = null;
            RefreshEmploymentCheckCommand secondCommand = null;

            _mockCommandDispatcher
                .Setup(m => m.SendMany(It.IsAny<IEnumerable<ICommand>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<ICommand>, CancellationToken>((c, t) =>
                {
                    firstCommand = c.First() as RefreshEmploymentCheckCommand;
                    secondCommand = c.Last() as RefreshEmploymentCheckCommand;
                });

            // Act
            await _sut.AddJob(request);

            // Assert
            _mockCommandDispatcher.Verify(m => m.SendMany(It.IsAny<IEnumerable<RefreshEmploymentCheckCommand>>(), It.IsAny<CancellationToken>()), Times.Once);
            firstCommand.ULN.Should().Be(uln1);
            firstCommand.AccountLegalEntityId.Should().Be(accountLegalEntityId1);
            firstCommand.ServiceRequestTaskId.Should().Be(serviceRequest1.TaskId);
            firstCommand.DecisionReference.Should().Be(serviceRequest1.DecisionReference);
            firstCommand.ServiceRequestCreated.Should().Be(serviceRequest1.TaskCreatedDate.Value);
            secondCommand.ULN.Should().Be(uln2);
            secondCommand.AccountLegalEntityId.Should().Be(accountLegalEntityId2);
            secondCommand.ServiceRequestTaskId.Should().Be(serviceRequest2.TaskId);
            secondCommand.DecisionReference.Should().Be(serviceRequest2.DecisionReference);
            secondCommand.ServiceRequestCreated.Should().Be(serviceRequest2.TaskCreatedDate.Value);
        }

        [Test]
        public async Task Then_the_application_service_request_overrides_the_request_level_service_request()
        {
            // Arrange
            var serviceRequest = _fixture.Create<ServiceRequest>();
            var accountLegalEntityId = _fixture.Create<long>();
            var uln = _fixture.Create<long>();
            var checkType = RefreshEmploymentCheckType.InitialEmploymentChecks.ToString();
            var applications = new List<Types.Application>
            {
                new Types.Application 
                {
                    AccountLegalEntityId = accountLegalEntityId, 
                    ULN = uln,
                    ServiceRequest = _fixture.Create<ServiceRequest>()
                }
            };
            var requests = new List<EmploymentCheckRefreshRequest>
            {
                new EmploymentCheckRefreshRequest
                {
                    Applications = applications.ToArray(),
                    ServiceRequest = serviceRequest,
                    CheckType = checkType
                }
            };
            var data = new Dictionary<string, object>
            {
                { "Requests", JsonConvert.SerializeObject(requests) }
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
            sentCommand.ServiceRequestTaskId.Should().Be(applications[0].ServiceRequest.TaskId);
            sentCommand.DecisionReference.Should().Be(applications[0].ServiceRequest.DecisionReference);
            sentCommand.ServiceRequestCreated.Should().Be(applications[0].ServiceRequest.TaskCreatedDate.Value);
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
            var requests = new List<EmploymentCheckRefreshRequest>
            {
                new EmploymentCheckRefreshRequest
                {
                    Applications = applications.ToArray(),
                    ServiceRequest = serviceRequest,
                    CheckType = checkType
                }
            };
            var data = new Dictionary<string, object>
            {
                { "Requests", JsonConvert.SerializeObject(requests) }
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

        [Test]
        public async Task Then_a_BadRequest_response_is_returned_if_the_incentive_does_not_exist()
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
            var requests = new List<EmploymentCheckRefreshRequest>
            {
                new EmploymentCheckRefreshRequest
                {
                    Applications = applications.ToArray(),
                    ServiceRequest = serviceRequest,
                    CheckType = checkType
                }
            };
            var data = new Dictionary<string, object>
            {
                { "Requests", JsonConvert.SerializeObject(requests) }
            };

            var request = _fixture.Build<JobRequest>()
                .With(r => r.Type, JobType.RefreshEmploymentChecks)
                .With(r => r.Data, data)
                .Create();

            _mockCommandDispatcher
                .Setup(m => m.SendMany(It.IsAny<IEnumerable<RefreshEmploymentCheckCommand>>(), It.IsAny<CancellationToken>()))
                .Throws(new ArgumentException("Incentive not found"));

            _sut = new JobCommandController(_mockCommandDispatcher.Object);

            // Act
            var actual = await _sut.AddJob(request) as BadRequestObjectResult;

            // Assert
            actual.Should().NotBeNull();
            actual.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            actual.Value.Should().Be("Incentive not found");
        }

        [Test]
        public async Task Then_a_BadRequest_response_is_returned_if_the_incentive_employment_checks_are_in_an_invalid_state()
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
            var requests = new List<EmploymentCheckRefreshRequest>
            {
                new EmploymentCheckRefreshRequest
                {
                    Applications = applications.ToArray(),
                    ServiceRequest = serviceRequest,
                    CheckType = checkType
                }
            };
            var data = new Dictionary<string, object>
            {
                { "Requests", JsonConvert.SerializeObject(requests) }
            };

            var request = _fixture.Build<JobRequest>()
                .With(r => r.Type, JobType.RefreshEmploymentChecks)
                .With(r => r.Data, data)
                .Create();

            _mockCommandDispatcher
                .Setup(m => m.SendMany(It.IsAny<IEnumerable<RefreshEmploymentCheckCommand>>(), It.IsAny<CancellationToken>()))
                .Throws(new InvalidOperationException("Employment checks not completed"));

            _sut = new JobCommandController(_mockCommandDispatcher.Object);

            // Act
            var actual = await _sut.AddJob(request) as BadRequestObjectResult;

            // Assert
            actual.Should().NotBeNull();
            actual.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            actual.Value.Should().Be("Employment checks not completed");
        }
    }
}
