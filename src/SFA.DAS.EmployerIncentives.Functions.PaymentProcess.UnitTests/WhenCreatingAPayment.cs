using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CreatePayment;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentProcess.UnitTests
{
    public class WhenCreatingAPayment
    {
        private Fixture _fixture;
        private CreatePayment _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            
            _sut = new CreatePayment(_mockCommandDispatcher.Object, Mock.Of<ILogger<CreatePayment>>());
        }

        [Test]
        public async Task Then_command_is_called_to_create_the_payment()
        {
            var input = _fixture.Create<CreatePaymentInput>();
            await _sut.Create(input);

            _mockCommandDispatcher.Verify(
                x => x.Send(
                    It.Is<CreatePaymentCommand>(p =>
                        p.ApprenticeshipIncentiveId == input.ApprenticeshipIncentiveId &&
                        p.PendingPaymentId == input.PendingPaymentId &&
                        p.CollectionPeriod == input.CollectionPeriod.Month &&
                        p.CollectionYear == input.CollectionPeriod.Year), CancellationToken.None), Times.Once);

        }
    }
}