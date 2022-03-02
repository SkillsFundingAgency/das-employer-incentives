using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Events.UnitTests.ApprenticeshipIncentives
{
    [TestFixture]
    public class WhenEarningsRecalculationRequiredEventHandled
    {
        private EarningsRecalculationRequiredHandler _sut;
        private Fixture _fixture;
        private Mock<ICommandPublisher> _commandPublisher;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _commandPublisher = new Mock<ICommandPublisher>();
            _sut = new EarningsRecalculationRequiredHandler(_commandPublisher.Object);
        }

        [Test]
        public async Task Then_a_command_is_published_to_request_calculation_of_earnings_for_the_incentive()
        {
            // Arrange
            var @event = _fixture.Create<EarningsRecalculationRequired>();

            // Act
            await _sut.Handle(@event);

            // Assert
            _commandPublisher.Verify(x => x.Publish(It.Is<CalculateEarningsCommand>(
                y => y.ApprenticeshipIncentiveId == @event.ApprenticeshipIncentiveId), 
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }
    }
}
