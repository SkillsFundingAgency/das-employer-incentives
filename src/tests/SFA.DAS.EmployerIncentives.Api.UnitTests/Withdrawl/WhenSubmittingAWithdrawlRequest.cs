using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.UpsertLegalEntity;
using SFA.DAS.EmployerIncentives.Commands.Withdrawls.EmployerWithdrawl;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.Withdrawl
{
    public class WhenSubmittingAWithdrawlRequest
    {
        private WithdrawlCommandController _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            _fixture = new Fixture();
            _sut = new WithdrawlCommandController(_mockCommandDispatcher.Object);

            _mockCommandDispatcher
                .Setup(m => m.Send(It.IsAny<UpsertLegalEntityCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Test]
        public async Task Then_a_EmployerWithdrawlCommand_command_is_dispatched_when_the_WithdrawlType_is_Employer()
        {
            // Arrange
            var request = _fixture
                .Build<WithdrawApplicationRequest>()
                .With(r => r.WithdrawlType, WithdrawlType.Employer)
                .Create();

            // Act
            await _sut.WithdrawlIncentiveApplication(request);

            // Assert
            _mockCommandDispatcher
                .Verify(m => m.Send(It.Is<EmployerWithdrawlCommand>(c => 
                    c.AccountLegalEntityId == request.AccountLegalEntityId &&
                    c.ULN == request.ULN && 
                    c.ServiceRequestTaskId == request.ServiceRequest.TaskId &&
                    c.ServiceRequestCreated == request.ServiceRequest.TaskCreatedDate.Value &&
                    c.DecisionReference == request.ServiceRequest.DecisionReference), 
                It.IsAny<CancellationToken>())
                ,Times.Once);                
        }

        [Test]
        public async Task Then_an_accepted_response_is_returned()
        {
            // Arrange
            var request = _fixture
                .Build<WithdrawApplicationRequest>()
                .With(r => r.WithdrawlType, WithdrawlType.Employer)
                .Create();

            // Act
            var actual = await _sut.WithdrawlIncentiveApplication(request) as AcceptedResult;

            // Assert
            actual.Should().NotBeNull();
        }
    }
}