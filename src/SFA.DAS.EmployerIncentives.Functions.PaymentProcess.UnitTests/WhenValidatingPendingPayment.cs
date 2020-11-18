using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentProcess.UnitTests
{
    public class WhenValidatingPendingPayment
    {
        private ValidatePendingPayment _sut;
        private Mock<ICommandDispatcher> _commandDispatcherMock;
        private Mock<ILogger<ValidatePendingPayment>> _loggerMock;
        private ValidatePendingPaymentData _payment;

        [SetUp]
        public async Task Setup()
        {
            var fixture = new Fixture();

            _commandDispatcherMock = new Mock<ICommandDispatcher>();
            _loggerMock = new Mock<ILogger<ValidatePendingPayment>>();
            _payment = fixture.Create<ValidatePendingPaymentData>();

            _sut = new ValidatePendingPayment(_commandDispatcherMock.Object, _loggerMock.Object);

            // Act
            await _sut.Validate(_payment);
        }

        [Test]
        public void Command_is_sent_to_validate_payment()
        {
            _commandDispatcherMock.Verify(x => x.Send(It.Is<ValidatePendingPaymentCommand>(c => c.ApprenticeshipIncentiveId == _payment.ApprenticeshipIncentiveId &&
                c.CollectionMonth == _payment.Month && c.CollectionYear == _payment.Year && c.PendingPaymentId == _payment.PendingPaymentId), CancellationToken.None), Times.Once);
        }
    }
}