using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.UnitTests.ApprenticeshipIncentives
{
    public class WhenCreatedHandler
    {
        private CreatedHandler _sut;
        private Mock<ICommandPublisher> _mockCommandPublisher;
        
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockCommandPublisher = new Mock<ICommandPublisher>();
            
            _sut = new CreatedHandler(_mockCommandPublisher.Object);
        }

        [Test]
        public async Task Then_a_CalculatePaymentsCommand_is_published()
        {
            //Arrange
            var @event = _fixture.Create<Created>();

            //Act
            await _sut.Handle(@event);

            //Assert
            _mockCommandPublisher.Verify(m => m.Publish(It.Is<CalculateEarningsCommand>(i => 
                i.AccountId == @event.AccountId &&
                i.ApprenticeshipIncentiveId == @event.ApprenticeshipIncentiveId &&
                i.ApprenticeshipId == @event.ApprenticeshipId), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
