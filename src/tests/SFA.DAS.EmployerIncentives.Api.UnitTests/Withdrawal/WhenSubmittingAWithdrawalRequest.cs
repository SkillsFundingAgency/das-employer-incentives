using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.UpsertLegalEntity;
using SFA.DAS.EmployerIncentives.Commands.Withdrawals.ComplianceWithdrawal;
using SFA.DAS.EmployerIncentives.Commands.Withdrawals.EmployerWithdrawal;
using SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.UlnHasPayments;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.Withdrawal
{
    public class WhenSubmittingAWithdrawalRequest
    {
        private WithdrawalCommandController _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;
        private Mock<IQueryDispatcher> _mockQueryDispatcher;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            _mockQueryDispatcher = new Mock<IQueryDispatcher>();
            _fixture = new Fixture();
            _sut = new WithdrawalCommandController(_mockCommandDispatcher.Object, _mockQueryDispatcher.Object);

            _mockCommandDispatcher
                .Setup(m => m.Send(It.IsAny<UpsertLegalEntityCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockQueryDispatcher
                .Setup(m => m.Send<UlnHasPaymentsRequest, UlnHasPaymentsResponse>(It.IsAny<UlnHasPaymentsRequest>()))
                .ReturnsAsync(new UlnHasPaymentsResponse(false));                
        }

        [Test]
        public async Task Then_a_EmployerWithdrawalCommand_command_is_dispatched_when_the_WithdrawalType_is_Employer()
        {
            // Arrange
            var request = _fixture
                .Build<WithdrawApplicationRequest>()
                .With(r => r.WithdrawalType, WithdrawalType.Employer)
                .Create();

            // Act
            await _sut.WithdrawalIncentiveApplication(request);

            // Assert
            _mockCommandDispatcher
                .Verify(m => m.Send(It.Is<EmployerWithdrawalCommand>(c => 
                    c.AccountLegalEntityId == request.AccountLegalEntityId &&
                    c.ULN == request.ULN && 
                    c.ServiceRequestTaskId == request.ServiceRequest.TaskId &&
                    c.ServiceRequestCreated == request.ServiceRequest.TaskCreatedDate.Value &&
                    c.DecisionReference == request.ServiceRequest.DecisionReference), 
                It.IsAny<CancellationToken>())
                ,Times.Once);                
        }

        [Test]
        public async Task Then_an_accepted_response_is_returned_when_the_WithdrawalType_is_Employer()
        {
            // Arrange
            var request = _fixture
                .Build<WithdrawApplicationRequest>()
                .With(r => r.WithdrawalType, WithdrawalType.Employer)
                .Create();

            // Act
            var actual = await _sut.WithdrawalIncentiveApplication(request) as AcceptedResult;

            // Assert
            actual.Should().NotBeNull();
        }

        [Test]
        public async Task Then_a_ComplianceWithdrawalCommand_command_is_dispatched_when_the_WithdrawalType_is_Compliance()
        {
            // Arrange
            var request = _fixture
                .Build<WithdrawApplicationRequest>()
                .With(r => r.WithdrawalType, WithdrawalType.Compliance)
                .Create();

            // Act
            await _sut.WithdrawalIncentiveApplication(request);

            // Assert
            _mockCommandDispatcher
                .Verify(m => m.Send(It.Is<ComplianceWithdrawalCommand>(c =>
                    c.AccountLegalEntityId == request.AccountLegalEntityId &&
                    c.ULN == request.ULN &&
                    c.ServiceRequestTaskId == request.ServiceRequest.TaskId &&
                    c.ServiceRequestCreated == request.ServiceRequest.TaskCreatedDate.Value &&
                    c.DecisionReference == request.ServiceRequest.DecisionReference),
                It.IsAny<CancellationToken>())
                , Times.Once);
        }

        [Test]
        public async Task Then_an_accepted_response_is_returned_when_the_WithdrawalType_is_Compliance()
        {
            // Arrange
            var request = _fixture
                .Build<WithdrawApplicationRequest>()
                .With(r => r.WithdrawalType, WithdrawalType.Compliance)
                .Create();

            // Act
            var actual = await _sut.WithdrawalIncentiveApplication(request) as AcceptedResult;

            // Assert
            actual.Should().NotBeNull();
        }
    }
}