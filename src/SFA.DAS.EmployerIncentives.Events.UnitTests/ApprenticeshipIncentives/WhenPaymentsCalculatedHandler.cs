using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.UnitTests.ApprenticeshipIncentives
{
    public class WhenEarningsCalculatedHandler
    {
        private EarningsCalculatedHandler _sut;
        private Mock<ICommandPublisher> _mockCommandPublisher;
        
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockCommandPublisher = new Mock<ICommandPublisher>();
            
            _sut = new EarningsCalculatedHandler(_mockCommandPublisher.Object);
        }

        [Test]
        public async Task Then_a_CompletePaymentsCalculationCommand_is_published()
        {
            //Arrange
            var @event = _fixture.Create<EarningsCalculated>();

            //Act
            await _sut.Handle(@event);

            //Assert
            _mockCommandPublisher.Verify(m => m.Publish(It.Is<CompleteEarningsCalculationCommand>(i => 
                i.AccountId == @event.AccountId &&
                i.ApprenticeshipIncentiveId == @event.ApprenticeshipIncentiveId &&
                i.ApprenticeshipId == @event.ApprenticeshipId &&
                i.IncentiveApplicationApprenticeshipId == @event.ApplicationApprenticeshipId), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
