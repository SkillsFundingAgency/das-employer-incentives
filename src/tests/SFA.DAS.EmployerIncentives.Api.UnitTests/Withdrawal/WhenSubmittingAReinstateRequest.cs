using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.Types.Withdrawals;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.Withdrawal
{
    public class WhenSubmittingAReinstateRequest
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
        }

        [Test]
        public async Task Then_a_ReinstateWithdrawalCommand_command_is_dispatched()
        {
            // Arrange
            var request = _fixture.Create<ReinstateApplicationRequest>();

            // Act
            await _sut.ReinstateIncentiveApplication(request);

            // Assert
            _mockCommandDispatcher
                .Verify(m => m.Send(It.Is<ReinstateWithdrawalCommand>(c => 
                    c.AccountLegalEntityId == request.AccountLegalEntityId &&
                    c.ULN == request.ULN), 
                It.IsAny<CancellationToken>())
                ,Times.Once);                
        }

        [Test]
        public async Task Then_an_accepted_response_is_returned()
        {
            // Arrange
            var request = _fixture.Create<ReinstateApplicationRequest>();

            // Act
            var actual = await _sut.ReinstateIncentiveApplication(request) as AcceptedResult;

            // Assert
            actual.Should().NotBeNull();
        }
    }
}