using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SendPaymentRequests;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentProcess.UnitTests
{
    public class WhenSendingPaymentsForALegalEntity
    {
        private Fixture _fixture;
        private SendPaymentRequestsForAccountLegalEntity _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            
            _sut = new SendPaymentRequestsForAccountLegalEntity(_mockCommandDispatcher.Object, Mock.Of<ILogger<SendPaymentRequestsForAccountLegalEntity>>());
        }

        [Test]
        public async Task Then_command_is_called_to_create_the_payment()
        {
            DateTime datePaid = DateTime.Now.AddYears(-1);
            _mockCommandDispatcher.Setup(x => x.Send(It.IsAny<SendPaymentRequestsCommand>(), CancellationToken.None))
                .Callback<SendPaymentRequestsCommand, CancellationToken>((cmd, ct) => datePaid = cmd.PaidDate);

            var input = _fixture.Create<AccountLegalEntityCollectionPeriod>();
            await _sut.Send(input);

            _mockCommandDispatcher.Verify(
                x => x.Send(
                    It.Is<SendPaymentRequestsCommand>(p =>
                        p.AccountLegalEntityId == input.AccountLegalEntityId), CancellationToken.None), Times.Once);

            datePaid.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
        }
    }
}