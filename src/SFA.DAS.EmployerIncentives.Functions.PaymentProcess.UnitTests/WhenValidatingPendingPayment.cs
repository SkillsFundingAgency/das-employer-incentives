using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentProcess.UnitTests
{
    public class WhenValidatingPendingPayment
    {
        private ValidatePendingPayment _sut;
        private Mock<ICommandDispatcher> _commandDispatcherMock;
        private ValidatePendingPaymentData _payment;

        [SetUp]
        public void Setup()
        {
            var fixture = new Fixture();

            _commandDispatcherMock = new Mock<ICommandDispatcher>();
            _payment = fixture.Create<ValidatePendingPaymentData>();

            _sut = new ValidatePendingPayment(_commandDispatcherMock.Object);
           
        }

        [Test]
        public async Task Command_is_sent_to_validate_payment()
        {
            // Act
            await _sut.Validate(_payment);

            // Assert
            _commandDispatcherMock.Verify(x => x.Send(It.Is<ValidatePendingPaymentCommand>(c => c.ApprenticeshipIncentiveId == _payment.ApprenticeshipIncentiveId &&
                c.CollectionPeriod == _payment.Period && c.CollectionYear == _payment.Year && c.PendingPaymentId == _payment.PendingPaymentId), CancellationToken.None), Times.Once);
        }

        [Test]
        public Task ValidatePendingPaymentException_is_thrown_on_error()
        {
            // Arrange
            var testException = new Exception(Guid.NewGuid().ToString());

            _commandDispatcherMock
                .Setup(m => m.Send(It.IsAny<ValidatePendingPaymentCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(testException);                

            //Act
            Func<Task> action = async () => await _sut.Validate(_payment);

            //Assert
            return action.Should().ThrowAsync<Exception>().WithMessage($"failed to validate ApprenticeshipIncentiveId : {_payment.ApprenticeshipIncentiveId}, PendingPaymentId : {_payment.PendingPaymentId}, Message : {testException.Message} ");
        }
    }
}