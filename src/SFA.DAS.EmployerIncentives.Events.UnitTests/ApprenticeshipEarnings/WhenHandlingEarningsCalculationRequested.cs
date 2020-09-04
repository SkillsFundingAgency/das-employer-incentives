using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.Apprenticeship;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events;
using SFA.DAS.EmployerIncentives.Events.ApprenticeshipEarnings;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Application.UnitTests.AddLegalEntity.Handlers
{
    public class WhenHandlingEarningsCalculationRequested
    {
        private EarningsCalculationRequestedHandler _sut;
        private Mock<ICommandPublisher> _mockCommandPublisher;
        
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockCommandPublisher = new Mock<ICommandPublisher>();
            
            _sut = new EarningsCalculationRequestedHandler(_mockCommandPublisher.Object);
        }

        [Test]
        public async Task Then_a_CalculateEarningsCommand_is_published()
        {
            //Arrange
            var @event = _fixture.Create<EarningsCalculationRequested>();

            //Act
            await _sut.Handle(@event);

            //Assert
            _mockCommandPublisher.Verify(m => m.Publish(It.Is<CalculateEarningsCommand>(i => 
                i.AccountId == @event.AccountId &&
                i.IncentiveClaimApprenticeshipId == @event.IncentiveClaimApprenticeshipId &&
                i.ApprenticeshipId == @event.ApprenticeshipId &&
                i.IncentiveType == @event.IncentiveType &&
                i.ApprenticeshipStartDate == @event.ApprenticeshipStartDate), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
