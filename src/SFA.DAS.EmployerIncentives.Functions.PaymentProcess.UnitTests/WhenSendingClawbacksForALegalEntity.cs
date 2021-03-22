using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SendClawbacks;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentProcess.UnitTests
{
    public class WhenSendingClawbacksForALegalEntity
    {
        private Fixture _fixture;
        private SendClawbacksForAccountLegalEntity _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            
            _sut = new SendClawbacksForAccountLegalEntity(_mockCommandDispatcher.Object, Mock.Of<ILogger<SendClawbacksForAccountLegalEntity>>());
        }

        [Test]
        public async Task Then_command_is_called_to_create_the_clawback()
        {
            DateTime datePaid = DateTime.Now.AddYears(-1);
            _mockCommandDispatcher.Setup(x => x.Send(It.IsAny<SendClawbacksCommand>(), CancellationToken.None))
                .Callback<SendClawbacksCommand, CancellationToken>((cmd, ct) => datePaid = cmd.ClawbackDate);

            var input = _fixture.Create<AccountLegalEntityCollectionPeriod>();
            await _sut.Send(input);

            _mockCommandDispatcher.Verify(
                x => x.Send(
                    It.Is<SendClawbacksCommand>(p =>
                        p.AccountLegalEntityId == input.AccountLegalEntityId), CancellationToken.None), Times.Once);

            datePaid.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
        }
    }
}