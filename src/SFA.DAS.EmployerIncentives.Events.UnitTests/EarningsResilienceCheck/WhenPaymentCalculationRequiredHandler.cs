using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.EarningsResilienceCheck.Events;
using SFA.DAS.EmployerIncentives.Events.EarningsResilienceCheck;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.UnitTests.EarningsResilienceCheck
{
    public class WhenPaymentCalculationRequiredHandler
    {
        private PaymentsCalculationRequiredHandler _sut;
        private Mock<ICommandPublisher> _mockCommandPublisher;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockCommandPublisher = new Mock<ICommandPublisher>();

            _sut = new PaymentsCalculationRequiredHandler(_mockCommandPublisher.Object);
        }

        [Test]
        public async Task Then_a_calculate_earnings_command_is_published_for_the_incentive()
        {
            //Arrange
            var @event = _fixture.Create<PaymentsCalculationRequired>();

            //Act
            await _sut.Handle(@event);

            //Assert
            _mockCommandPublisher.Verify(x => x.Publish(It.Is<CalculateEarningsCommand>(x => x.ApprenticeshipIncentiveId == @event.Model.Id), It.IsAny<CancellationToken>()), Times.Once);
            
        }
    }
}
