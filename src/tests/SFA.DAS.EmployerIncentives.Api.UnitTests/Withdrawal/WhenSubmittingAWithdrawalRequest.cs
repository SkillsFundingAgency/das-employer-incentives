using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.Types.Withdrawals;
using SFA.DAS.EmployerIncentives.Commands.UpsertLegalEntity;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.Withdrawal
{
    public class WhenSubmittingAWithdrawalRequest
    {
        private WithdrawalCommandController _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            _fixture = new Fixture();
            _sut = new WithdrawalCommandController(_mockCommandDispatcher.Object);

            _mockCommandDispatcher
                .Setup(m => m.Send(It.IsAny<UpsertLegalEntityCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Test]
        public async Task Then_a_EmployerWithdrawalCommand_command_is_dispatched_when_the_WithdrawalType_is_Employer()
        {
            // Arrange
            var request = _fixture
                .Build<WithdrawApplicationRequest>()
                .With(r => r.WithdrawalType, WithdrawalType.Employer)
                .With(r => r.Applications, _fixture.CreateMany<Types.Application>(1).ToArray())
                .Create();

            EmployerWithdrawalCommand sentCommand = null;

            _mockCommandDispatcher
                .Setup(m => m.SendMany(It.IsAny<IEnumerable<ICommand>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<ICommand>, CancellationToken>((c, t) =>
                {
                    sentCommand = c.Single() as EmployerWithdrawalCommand;
                });

            // Act
            await _sut.WithdrawalIncentiveApplication(request);

            // Assert
            _mockCommandDispatcher
                .Verify(m => m.SendMany(It.Is<List<EmployerWithdrawalCommand>>(c => c.Count == 1),
                        It.IsAny<CancellationToken>())
                    , Times.Once);

            sentCommand.AccountLegalEntityId.Should().Be(request.Applications[0].AccountLegalEntityId);
            sentCommand.ULN.Should().Be(request.Applications[0].ULN);
            sentCommand.AccountId.Should().Be(request.AccountId);
            sentCommand.EmailAddress.Should().Be(request.EmailAddress);
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
        public async Task Then_a_ComplianceWithdrawalCommand_commands_are_dispatched_when_the_WithdrawalType_is_Compliance()
        {
            // Arrange
            var request = _fixture
                .Build<WithdrawApplicationRequest>()
                .With(r => r.WithdrawalType, WithdrawalType.Compliance)
                .With(r => r.Applications, _fixture.CreateMany<Types.Application>(2).ToArray())
                .Create();

            ComplianceWithdrawalCommand sentCommand1 = null;
            ComplianceWithdrawalCommand sentCommand2 = null;

            _mockCommandDispatcher
                .Setup(m => m.SendMany(It.IsAny<IEnumerable<ICommand>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<ICommand>, CancellationToken>((c, t) =>
                {
                    sentCommand1 = c.First() as ComplianceWithdrawalCommand;
                    sentCommand2 = c.Last() as ComplianceWithdrawalCommand;
                });

            // Act
            await _sut.WithdrawalIncentiveApplication(request);

            // Assert
            _mockCommandDispatcher
                .Verify(m => m.SendMany(It.Is<List<ComplianceWithdrawalCommand>>(c => c.Count == 2),
                        It.IsAny<CancellationToken>())
                    , Times.Once);

            sentCommand1.AccountLegalEntityId.Should().Be(request.Applications[0].AccountLegalEntityId);
            sentCommand1.ULN.Should().Be(request.Applications[0].ULN);
            sentCommand1.ServiceRequestTaskId.Should().Be(request.ServiceRequest.TaskId);
            sentCommand1.ServiceRequestCreated.Should().Be(request.ServiceRequest.TaskCreatedDate.Value);
            sentCommand1.DecisionReference.Should().Be(request.ServiceRequest.DecisionReference);
            
            sentCommand2.AccountLegalEntityId.Should().Be(request.Applications[1].AccountLegalEntityId);
            sentCommand2.ULN.Should().Be(request.Applications[1].ULN);
            sentCommand2.ServiceRequestTaskId.Should().Be(request.ServiceRequest.TaskId);
            sentCommand2.ServiceRequestCreated.Should().Be(request.ServiceRequest.TaskCreatedDate.Value);
            sentCommand2.DecisionReference.Should().Be(request.ServiceRequest.DecisionReference);
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